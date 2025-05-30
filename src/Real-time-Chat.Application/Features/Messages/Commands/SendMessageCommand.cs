using MediatR;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Application.Interfaces;
using Real_time_Chat.Domain.Entities;

namespace Real_time_Chat.Application.Features.Messages.Commands;

public sealed record SendMessageCommand(
    string Content,
    Guid ChatId) : IRequest<SendMessageCommandResponse>;

public sealed record SendMessageCommandResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset SendAt { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUserName { get; set; } = string.Empty;
    public Guid ChatId { get; set; }
}

internal sealed class SendMessageCommandHandler(IApplicationDbContext applicationDbContext,
ICurrentUserService currentUserService) : IRequestHandler<SendMessageCommand, SendMessageCommandResponse>
{
    public async Task<SendMessageCommandResponse> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = currentUserService.UserId;

        // Verify chat exists and user is a member
        var isUserInChat = await applicationDbContext.UserChats
            .AnyAsync(uc => uc.ChatId == request.ChatId &&
                           uc.UserId == senderId,
                    cancellationToken);

        if (!isUserInChat)
        {
            throw new UnauthorizedAccessException("User is not a member of this chat");
        }

        var message = new Message
        {
            Content = request.Content,
            ChatId = request.ChatId,
            SenderId = senderId,
            SendAt = DateTimeOffset.UtcNow
        };

        applicationDbContext.Messages.Add(message);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        // Reload the message with sender information
        return await applicationDbContext.Messages
            .Where(m => m.Id == message.Id)
            .Select(m => new SendMessageCommandResponse
            {
                Id = m.Id,
                Content = m.Content,
                SendAt = m.SendAt,
                SenderId = m.SenderId,
                SenderUserName = m.Sender.UserName!,
                ChatId = m.ChatId
            })
            .FirstOrDefaultAsync(cancellationToken) ?? new SendMessageCommandResponse();
    }
}
