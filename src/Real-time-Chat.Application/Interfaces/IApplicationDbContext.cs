using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Domain.Entities;

namespace Real_time_Chat.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Message> Messages { get; }
        DbSet<Chat> Chats { get; }
        DbSet<UserChat> UserChats { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
