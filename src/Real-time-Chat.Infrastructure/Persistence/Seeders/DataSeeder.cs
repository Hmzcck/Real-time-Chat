using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Domain.Entities;
using Real_time_Chat.Infrastructure.Persistence.Contexts;

namespace Real_time_Chat.Infrastructure.Persistence.Seeders;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly int _userCount = 10;
    private readonly int _groupChatCount = 3;
    private readonly int _messagesPerChat = 20;

    public DataSeeder(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<SeederResponse> SeedAsync()
    {
        if (await _context.Users.AnyAsync())
            return new SeederResponse 
            {
                Users = new List<UserInfo>(),
                Groups = new List<GroupInfo>()
            };

        var users = await SeedUsers();
        var groupChats = await SeedGroupChats(users);
        var privateChats = await SeedPrivateChats(users);
        await SeedMessages(users, groupChats.Concat(privateChats).ToList());
        
        await _context.SaveChangesAsync();

        return new SeederResponse
        {
            Users = users.Select(u => new UserInfo
            {
                UserName = u.UserName!,
                Email = u.Email!,
                AvatarPath = u.AvatarPath
            }).ToList(),
            Groups = groupChats.Select(g => new GroupInfo
            {
                Name = g.Name,
                ImagePath = g.ImagePath!,
                Members = g.UserChats.Select(uc => 
                    users.First(u => u.Id == uc.UserId).UserName!).ToList()
            }).ToList()
        };
    }

    private async Task<List<User>> SeedUsers()
    {
        var users = new List<User>();
        
        for (int i = 1; i <= _userCount; i++)
        {
            var faker = new Faker();
            var username = faker.Internet.UserName();
            var user = new User
            {
                UserName = username,
                Email = faker.Internet.Email(username),
                AvatarPath = $"user{i}.png",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            users.Add(user);
        }
        const string defaultPassword = "123";

        foreach (var user in users)
        {
            await _userManager.CreateAsync(user, defaultPassword);
        }

        return users;
    }

    private async Task<List<Chat>> SeedGroupChats(List<User> users)
    {
        var groupChats = new List<Chat>();
        
        for (int i = 1; i <= _groupChatCount; i++)
        {
            var faker = new Faker();
            var chat = new Chat
            {
                Id = Guid.NewGuid(),
                Name = faker.Company.CompanyName(),
                IsPrivate = false,
                ImagePath = $"group{i}.png"
            };
            groupChats.Add(chat);
        }

        foreach (var chat in groupChats)
        {
            // Add random users to each group chat
            var randomUsers = users.OrderBy(_ => Guid.NewGuid()).Take(5).ToList();
            chat.UserChats = randomUsers.Select(u => new UserChat
            {
                UserId = u.Id,
                ChatId = chat.Id
            }).ToList();
        }

        await _context.Chats.AddRangeAsync(groupChats);
        return groupChats;
    }

    private async Task<List<Chat>> SeedPrivateChats(List<User> users)
    {
        var privateChats = new List<Chat>();

        // Create private chats between each pair of users
        for (int i = 0; i < users.Count; i++)
        {
            for (int j = i + 1; j < users.Count; j++)
            {
                var chat = new Chat
                {
                    Id = Guid.NewGuid(),
                    Name = $"Private Chat",
                    IsPrivate = true,
                    UserChats = new List<UserChat>
                    {
                        new() { UserId = users[i].Id },
                        new() { UserId = users[j].Id }
                    }
                };

                privateChats.Add(chat);
            }
        }

        await _context.Chats.AddRangeAsync(privateChats);
        return privateChats;
    }

    private async Task SeedMessages(List<User> users, List<Chat> chats)
    {
        var messageFaker = new Faker<Message>()
            .RuleFor(m => m.Id, _ => Guid.NewGuid())
            .RuleFor(m => m.Content, f => f.Lorem.Sentence())
            .RuleFor(m => m.SendAt, f => DateTimeOffset.UtcNow.AddDays(-f.Random.Int(1, 7)));

        foreach (var chat in chats)
        {
            // Get the users directly from the chat's UserChats collection
            var chatUserIds = chat.UserChats.Select(uc => uc.UserId).ToList();

            var messages = messageFaker
                .RuleFor(m => m.ChatId, _ => chat.Id)
                .RuleFor(m => m.SenderId, f => f.PickRandom(chatUserIds))
                .Generate(_messagesPerChat);

            await _context.Messages.AddRangeAsync(messages);
        }
    }
}
