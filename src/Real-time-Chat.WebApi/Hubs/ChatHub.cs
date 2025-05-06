using Microsoft.AspNetCore.SignalR;
using MediatR;
using Real_time_Chat.Application.Features.Messages.Commands;
using Real_time_Chat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Real_time_Chat.WebApi.Hubs;

// [Authorize]
public class ChatHub(IMediator _mediator,
UserConnectionManager _connectionManager, ICurrentUserService _currentUserService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = _currentUserService.UserId;
        await _connectionManager.SaveConnectionAsync(userId, Context.ConnectionId);
        await Clients.All.SendAsync("UserOnline", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUserService.UserId;
        await _connectionManager.RemoveConnectionAsync(userId, Context.ConnectionId);

        // Only notify if user has no other active connections
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

    public async Task SendMessage(Guid chatId, string content)
    {
        var command = new SendMessageCommand(content, chatId);
        var result = await _mediator.Send(command);

        // Send to all connections in the chat group
        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", result);
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
