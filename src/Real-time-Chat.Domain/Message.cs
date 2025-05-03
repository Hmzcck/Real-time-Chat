namespace Real_time_Chat.Domain;

public class Message
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset SendAt { get; set; }

    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
}
