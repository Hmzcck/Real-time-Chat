using MediatR;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Application.Interfaces;

namespace Real_time_Chat.Application.Features.Chats.Queries;

public sealed record GetChatsQuery() : IRequest<List<GetChatsQueryResponse>>;

public sealed record GetChatsQueryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }
    public string? LastMessage { get; set; }
    public string? LastMessageSender { get; set; }
    public int MemberCount { get; set; }
    public string? ImagePath { get; set; }
    public List<string> Members { get; set; } = [];
}

internal sealed class GetChatsQueryHandler(IApplicationDbContext applicationDbContext,
ICurrentUserService currentUserService) : IRequestHandler<GetChatsQuery, List<GetChatsQueryResponse>>
{
    public async Task<List<GetChatsQueryResponse>> Handle(GetChatsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        return await applicationDbContext.Chats
            .Where(c => c.UserChats.Any(uc => uc.UserId == userId))
            .Select(c => new GetChatsQueryResponse
            {
                Id = c.Id,
                Name = !c.IsPrivate ? c.Name :
                    c.UserChats
                        .Where(uc => uc.UserId != userId)
                        .Select(uc => uc.User.UserName)
                        .FirstOrDefault() ?? "",
                IsPrivate = c.IsPrivate,
                LastMessageAt = c.Messages
                    .OrderByDescending(m => m.SendAt)
                    .Select(m => (DateTimeOffset?)m.SendAt)
                    .FirstOrDefault(),
                LastMessage = c.Messages
                    .OrderByDescending(m => m.SendAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),
                LastMessageSender = c.Messages
                    .OrderByDescending(m => m.SendAt)
                    .Select(m => m.Sender.UserName)
                    .FirstOrDefault(),
                MemberCount = c.UserChats.Count,
                ImagePath = !c.IsPrivate ? c.ImagePath :
                    c.UserChats
                        .Where(uc => uc.UserId != userId)
                        .Select(uc => uc.User.AvatarPath)
                        .FirstOrDefault(),
                Members = c.UserChats
                    .Select(uc => uc.User.UserName ?? string.Empty)
                    .ToList(),
            })
            .OrderByDescending(c => c.LastMessageAt ?? DateTimeOffset.MinValue) // Handles nulls
            .ToListAsync(cancellationToken);
    }
}
