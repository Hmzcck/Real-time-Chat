using Microsoft.AspNetCore.SignalR;
using MediatR;
using Real_time_Chat.Application.Features.Messages.Commands;
using Real_time_Chat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Real_time_Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Real_time_Chat.WebApi.Hubs;

[Authorize]
public class ChatHub(IMediator _mediator,
UserConnectionManager _connectionManager, ICurrentUserService _currentUserService, UserManager<User> userManager) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Guid.Parse(userManager.GetUserId(Context.User));

        await _connectionManager.SaveConnectionAsync(userId, Context.ConnectionId);

        await Clients.Others.SendAsync("UserOnline", userId);

        var onlineUsers = await _connectionManager.GetAllOnlineUsersAsync();

        onlineUsers = onlineUsers.Where(id => id != userId).ToList();

        await Clients.Caller.SendAsync("OnlineUsersList", onlineUsers);

        await base.OnConnectedAsync();
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdString = userManager.GetUserId(Context.User);

        if (string.IsNullOrEmpty(userIdString))
        {
            return;
        }

        var userId = Guid.Parse(userIdString);
        await _connectionManager.RemoveAllConnectionsAsync(userId);

        if (!await _connectionManager.IsUserOnlineAsync(userId))
        {
            await Clients.All.SendAsync("UserOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }


    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    private async Task<List<Guid>> GetChatParticipantIds(Guid chatId)
    {
        // TODO use repository pattern
        var db = Context.GetHttpContext().RequestServices.GetRequiredService<IApplicationDbContext>();
        return await db.UserChats
            .Where(uc => uc.ChatId == chatId)
            .Select(uc => uc.UserId)
            .ToListAsync();
    }

    public async Task SendMessage(Guid chatId, string content)
    {
        var command = new SendMessageCommand(content, chatId);
        var result = await _mediator.Send(command);

        // Get all user connections in the chat
        var connections = await _connectionManager.GetChatUserConnectionsAsync(chatId, GetChatParticipantIds);

        foreach (var connectionId in connections)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", result);
        }
    }

    public async Task UserTyping(Guid chatId, string username)
    {
        // Notify all users in the chat except the sender
        await Clients.GroupExcept(chatId.ToString(), Context.ConnectionId)
            .SendAsync("UserTyping", username);
    }

    public async Task UserStoppedTyping(Guid chatId, string username)
    {
        // Notify all users in the chat except the sender
        await Clients.GroupExcept(chatId.ToString(), Context.ConnectionId)
            .SendAsync("UserStoppedTyping", username);
    }
}
