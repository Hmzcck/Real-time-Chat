using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Real_time_Chat.WebApi.Hubs;

public class UserConnectionManager(IDistributedCache _cache)
{
    private const string KeyPrefix = "user:connection:";
    private const string OnlineUsersKey = "online:users";
    private const int CacheExpirationMinutes = 1440; // 24 hours

    public async Task SaveConnectionAsync(Guid userId, string connectionId)
    {
        var connections = await GetUserConnectionsAsync(userId);
        if (!connections.Contains(connectionId))
        {
            connections.Add(connectionId);
        }

        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));

        await _cache.SetStringAsync(
            $"{KeyPrefix}{userId}",
            JsonSerializer.Serialize(connections),
            options);

        await AddUserToOnlineListAsync(userId);
    }

    public async Task RemoveConnectionAsync(Guid userId, string connectionId)
    {
        var connections = await GetUserConnectionsAsync(userId);
        connections.Remove(connectionId);

        if (connections.Count > 0)
        {
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));

            await _cache.SetStringAsync(
                $"{KeyPrefix}{userId}",
                JsonSerializer.Serialize(connections),
                options);
        }
        else
        {
            await _cache.RemoveAsync($"{KeyPrefix}{userId}");
            await RemoveUserFromOnlineListAsync(userId);
        }
    }

    public async Task RemoveAllConnectionsAsync(Guid userId)
    {
        await _cache.RemoveAsync($"{KeyPrefix}{userId}");
        await RemoveUserFromOnlineListAsync(userId);
    }


    public async Task<List<string>> GetUserConnectionsAsync(Guid userId)
    {
        var connectionsJson = await _cache.GetStringAsync($"{KeyPrefix}{userId}");
        if (string.IsNullOrEmpty(connectionsJson))
        {
            return new List<string>();
        }

        return JsonSerializer.Deserialize<List<string>>(connectionsJson) ?? new List<string>();
    }

    public async Task<bool> IsUserOnlineAsync(Guid userId)
    {
        var connections = await GetUserConnectionsAsync(userId);
        return connections.Count > 0;
    }

    public async Task<List<string>> GetChatUserConnectionsAsync(Guid chatId, Func<Guid, Task<List<Guid>>> getChatParticipantIds)
    {
        var connections = new List<string>();
        var participantIds = await getChatParticipantIds(chatId);

        foreach (var userId in participantIds)
        {
            var userConnections = await GetUserConnectionsAsync(userId);
            connections.AddRange(userConnections);
        }

        return connections;
    }

    public async Task<List<Guid>> GetAllOnlineUsersAsync()
    {
        return await GetOnlineUserIdsAsync();
    }

    // PRIVATE METHODS BELOW

    private async Task AddUserToOnlineListAsync(Guid userId)
    {
        var onlineUsers = await GetOnlineUserIdsAsync();
        if (!onlineUsers.Contains(userId))
        {
            onlineUsers.Add(userId);
            await _cache.SetStringAsync(OnlineUsersKey, JsonSerializer.Serialize(onlineUsers));
        }
    }

    private async Task RemoveUserFromOnlineListAsync(Guid userId)
    {
        var onlineUsers = await GetOnlineUserIdsAsync();
        if (onlineUsers.Contains(userId))
        {
            onlineUsers.Remove(userId);
            await _cache.SetStringAsync(OnlineUsersKey, JsonSerializer.Serialize(onlineUsers));
        }
    }

    private async Task<List<Guid>> GetOnlineUserIdsAsync()
    {
        var json = await _cache.GetStringAsync(OnlineUsersKey);
        if (string.IsNullOrEmpty(json))
            return new List<Guid>();

        return JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
    }
}
