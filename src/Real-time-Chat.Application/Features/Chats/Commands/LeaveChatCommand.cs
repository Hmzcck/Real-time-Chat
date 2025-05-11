using MediatR;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Application.Interfaces;

namespace Real_time_Chat.Application.Features.Chats.Commands;

public sealed record LeaveChatCommand(Guid ChatId) : IRequest<LeaveChatCommandResponse>;

public sealed record LeaveChatCommandResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Guid ChatId { get; set; }
    public Guid UserId { get; set; }
}

public class LeaveChatCommandHandler(IApplicationDbContext applicationDbContext, ICurrentUserService currentUserService) : IRequestHandler<LeaveChatCommand, LeaveChatCommandResponse>
{
    public async Task<LeaveChatCommandResponse> Handle(LeaveChatCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var chat = await applicationDbContext.Chats
            .Include(c => c.UserChats)
            .FirstOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

        if (chat == null)
        {
            return new LeaveChatCommandResponse 
            { 
                Success = false, 
                Error = "Chat not found",
                ChatId = request.ChatId,
                UserId = userId
            };
        }

        if (chat.IsPrivate)
        {
            return new LeaveChatCommandResponse 
            { 
                Success = false, 
                Error = "Cannot leave private chats",
                ChatId = request.ChatId,
                UserId = userId
            };
        }

        var userChat = chat.UserChats.FirstOrDefault(uc => uc.UserId == userId);
        if (userChat == null)
        {
            return new LeaveChatCommandResponse 
            { 
                Success = false, 
                Error = "User is not a member of this chat",
                ChatId = request.ChatId,
                UserId = userId
            };
        }

        applicationDbContext.UserChats.Remove(userChat);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return new LeaveChatCommandResponse 
        { 
            Success = true,
            ChatId = request.ChatId,
            UserId = userId
        };
    }
}
