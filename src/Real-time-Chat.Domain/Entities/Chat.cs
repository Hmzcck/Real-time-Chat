namespace Real_time_Chat.Domain.Entities;

public class Chat
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsPrivate { get; set; }
    public string? ImagePath { get; set; }

    public ICollection<UserChat> UserChats { get; set; } = default!;
    public ICollection<Message> Messages { get; set; } = default!; 
}
