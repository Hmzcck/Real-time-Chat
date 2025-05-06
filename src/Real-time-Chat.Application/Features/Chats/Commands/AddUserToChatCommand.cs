using MediatR;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Application.Interfaces;
using Real_time_Chat.Domain.Entities;

namespace Real_time_Chat.Application.Features.Chats.Commands;

public sealed record AddUserToChatCommand(
    Guid ChatId,
    Guid UserId) : IRequest<bool>;

internal sealed class AddUserToChatCommandHandler(IApplicationDbContext applicationDbContext,
ICurrentUserService currentUserService) : IRequestHandler<AddUserToChatCommand, bool>
{
    public async Task<bool> Handle(AddUserToChatCommand request, CancellationToken cancellationToken)
    {
        var requesterId = currentUserService.UserId;

        // Verify requester is a member of the chat
        var isRequesterInChat = await applicationDbContext.UserChats
            .AnyAsync(uc => uc.ChatId == request.ChatId &&
                           uc.UserId == requesterId,
                    cancellationToken);

        if (!isRequesterInChat)
        {
            throw new UnauthorizedAccessException("User is not authorized to add members to this chat");
        }

        // Check if user is already in chat
        var isUserAlreadyInChat = await applicationDbContext.UserChats
            .AnyAsync(uc => uc.ChatId == request.ChatId &&
                           uc.UserId == request.UserId,
                    cancellationToken);

        if (isUserAlreadyInChat)
        {
            return false;
        }

        // Add user to chat
        var userChat = new UserChat
        {
            ChatId = request.ChatId,
            UserId = request.UserId,
            JoinedAt = DateTimeOffset.UtcNow
        };

        applicationDbContext.UserChats.Add(userChat);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
