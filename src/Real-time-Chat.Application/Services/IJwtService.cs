using Real_time_Chat.Domain.Entities;

namespace Real_time_Chat.Application.Services;

public interface IJwtService
{
    public Task<string> CreateTokenAsync(User user, CancellationToken cancellationToken = default);
}