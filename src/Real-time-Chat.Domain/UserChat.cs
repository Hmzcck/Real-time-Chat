
namespace Real_time_Chat.Domain;

public class UserChat
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public DateTimeOffset JoinedAt { get; set; }
}
