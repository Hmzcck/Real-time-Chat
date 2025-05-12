namespace Real_time_Chat.Infrastructure.Persistence.Seeders;

public class UserInfo
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string AvatarPath { get; set; }
}

public class GroupInfo
{
    public required string Name { get; set; }
    public required string ImagePath { get; set; }
    public required List<string> Members { get; set; }
}

public class SeederResponse
{
    public required List<UserInfo> Users { get; set; }
    public required List<GroupInfo> Groups { get; set; }
}
