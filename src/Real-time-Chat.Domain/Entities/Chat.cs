namespace Real_time_Chat.Domain.Entities;

public class Chat
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }

    public ICollection<UserChat> UserChats { get; set; } = new List<UserChat>();
    public ICollection<Message> Messages { get; set; } = new List<Message>(); 
}
