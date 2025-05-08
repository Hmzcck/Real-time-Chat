using MediatR;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Application.Interfaces;

namespace Real_time_Chat.Application.Features.Messages.Queries;

public sealed record GetMessagesQuery(Guid ChatId) : IRequest<List<GetMessageQueryResponse>>;

public sealed record GetMessageQueryResponse
{
    public Guid SenderId { get; set; }
    public string SenderUserName { get; set; } = string.Empty;
    public Guid ChatId { get; set; }
    public DateTimeOffset SendAt { get; set; }
    public string Content { get; set; } = string.Empty;
}

internal sealed class GetMessagesQueryHandler(IApplicationDbContext applicationDbContext) : IRequestHandler<GetMessagesQuery, List<GetMessageQueryResponse>>
{
    public async Task<List<GetMessageQueryResponse>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        return await applicationDbContext.Messages
            .Include(m => m.Sender)
            .Where(m => m.ChatId == request.ChatId)
            .Select(m => new GetMessageQueryResponse
            {
                SenderId = m.SenderId,
                SenderUserName = m.Sender.UserName!,
                ChatId = m.ChatId,
                SendAt = m.SendAt,
                Content = m.Content
            })
            .ToListAsync(cancellationToken);
    }
}
