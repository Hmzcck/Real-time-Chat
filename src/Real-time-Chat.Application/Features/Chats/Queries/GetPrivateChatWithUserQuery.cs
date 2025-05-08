using MediatR;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Application.Interfaces;

namespace Real_time_Chat.Application.Features.Chats.Queries;

public sealed record GetPrivateChatWithUserQuery(Guid OtherUserId) : IRequest<GetChatDetailsQueryResponse>;

internal sealed class GetPrivateChatWithUserQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService
) : IRequestHandler<GetPrivateChatWithUserQuery, GetChatDetailsQueryResponse?>
{
    public async Task<GetChatDetailsQueryResponse?> Handle(GetPrivateChatWithUserQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId;

        var chat = await context.Chats
            .Where(c => c.IsPrivate)
            .Where(c =>
                c.UserChats.Any(uc => uc.UserId == currentUserId) &&
                c.UserChats.Any(uc => uc.UserId == request.OtherUserId))
            .Select(c => new GetChatDetailsQueryResponse
            {
                Id = c.Id,
                Name = c.Name,
                IsPrivate = c.IsPrivate,
                LastMessageAt = c.Messages
                    .OrderByDescending(m => m.SendAt)
                    .Select(m => m.SendAt)
                    .FirstOrDefault(),
                Members = c.UserChats.Select(uc => new ChatMemberResponse
                {
                    UserId = uc.UserId,
                    Username = uc.User.UserName
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return chat;
    }
}
