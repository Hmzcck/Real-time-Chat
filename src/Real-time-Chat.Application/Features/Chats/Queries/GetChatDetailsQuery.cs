using MediatR;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Application.Interfaces;

namespace Real_time_Chat.Application.Features.Chats.Queries;

public sealed record GetChatDetailsQuery(
    Guid ChatId) : IRequest<GetChatDetailsQueryResponse>;

public sealed record ChatMemberResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
}

public sealed record GetChatDetailsQueryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }
    public List<ChatMemberResponse> Members { get; set; } = new();
}

internal sealed class GetChatDetailsQueryHandler(IApplicationDbContext applicationDbContext,
ICurrentUserService currentUserService) : IRequestHandler<GetChatDetailsQuery, GetChatDetailsQueryResponse>
{
    public async Task<GetChatDetailsQueryResponse> Handle(GetChatDetailsQuery request, CancellationToken cancellationToken)
    {
        var requesterId = currentUserService.UserId;

        // Verify requester is a member of the chat
        var isRequesterInChat = await applicationDbContext.UserChats
            .AnyAsync(uc => uc.ChatId == request.ChatId &&
                           uc.UserId == requesterId,
                    cancellationToken);

        if (!isRequesterInChat)
        {
            throw new UnauthorizedAccessException("User is not authorized to view this chat");
        }

        var chatDetails = await applicationDbContext.Chats
            .Where(c => c.Id == request.ChatId)
            .Select(c => new GetChatDetailsQueryResponse
            {
                Id = c.Id,
                Name = c.Name,
                IsPrivate = c.IsPrivate,
                LastMessageAt = c.Messages
                    .OrderByDescending(m => m.SendAt)
                    .Select(m => m.SendAt)
                    .FirstOrDefault(),
                Members = c.UserChats
                    .Select(uc => new ChatMemberResponse
                    {
                        UserId = uc.UserId,
                        Username = uc.User.UserName ?? string.Empty
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (chatDetails == null)
        {
            throw new KeyNotFoundException("Chat not found");
        }

        return chatDetails;
    }
}
