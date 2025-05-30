using MediatR;
using Real_time_Chat.Application.Interfaces;
using Real_time_Chat.Domain.Entities;

namespace Real_time_Chat.Application.Features.Chats.Commands;

public sealed record CreateChatCommand(
    string Name,
    bool IsPrivate,
    string? ImagePath,
    List<Guid> InitialMemberIds) : IRequest<CreateChatCommandResponse>;

public sealed record CreateChatCommandResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public string? ImagePath { get; set; }
}

internal sealed class CreateChatCommandHandler(IApplicationDbContext applicationDbContext,
ICurrentUserService currentUserService) : IRequestHandler<CreateChatCommand, CreateChatCommandResponse>
{
    public async Task<CreateChatCommandResponse> Handle(CreateChatCommand request, CancellationToken cancellationToken)
    {
        var creatorId = currentUserService.UserId;

        var chat = new Chat
        {
            Name = request.Name,
            IsPrivate = request.IsPrivate,
            ImagePath = request.IsPrivate ? null : (request.ImagePath ?? "default_group_image.png"),
        };

        // Add creator and initial members
        var members = request.InitialMemberIds
            .Union([creatorId])
            .Distinct()
            .Select(userId => new UserChat
            {
                UserId = userId,
                ChatId = chat.Id,
                JoinedAt = DateTimeOffset.UtcNow
            })
            .ToList();

        chat.UserChats = members;

        applicationDbContext.Chats.Add(chat);
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return new CreateChatCommandResponse
        {
            Id = chat.Id,
            Name = chat.Name,
            IsPrivate = chat.IsPrivate
        };
    }
}
