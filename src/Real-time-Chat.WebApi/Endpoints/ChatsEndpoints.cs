using MediatR;
using Microsoft.AspNetCore.Authorization;
using Real_time_Chat.Application.Features.Chats.Commands;
using Real_time_Chat.Application.Features.Chats.Queries;

namespace Real_time_Chat.WebApi.Endpoints;

// [Authorize]
public static class ChatsEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/chats")
            .WithTags("Chats")
            .WithOpenApi();

        // Get all chats
        group.MapGet("/", async (IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetChatsQuery());
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetChats")
        .WithDescription("Returns all chats for the current user")
        .Produces<List<GetChatsQueryResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // Get chat details
        group.MapGet("/{id}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetChatDetailsQuery(id));
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Problem("Unauthorized access", statusCode: StatusCodes.Status403Forbidden);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetChatDetails")
        .WithDescription("Returns detailed information about a specific chat")
        .Produces<GetChatDetailsQueryResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // Create new chat
        group.MapPost("/", async (CreateChatCommand request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(request);
                return Results.Created($"/api/chats/{result.Id}", result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("CreateChat")
        .WithDescription("Creates a new chat")
        .Produces<CreateChatCommandResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // Add user to chat
        group.MapPost("/{chatId}/users", async (Guid chatId, AddUserToChatCommand request, IMediator mediator) =>
        {
            if (chatId != request.ChatId)
            {
                return Results.BadRequest("Chat ID in route must match body");
            }

            try
            {
                await mediator.Send(request);
                return Results.Ok();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("AddUserToChat")
        .WithDescription("Adds a user to an existing chat")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/private/{userId}", async(Guid UserId, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetPrivateChatWithUserQuery(UserId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetPrivateChatWithUser")
        .WithDescription("Returns the private chat with the specified user")
        .Produces<GetChatDetailsQueryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // Leave chat
        group.MapDelete("/{chatId}/leave", async (Guid chatId, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new LeaveChatCommand(chatId));
                if (!result.Success)
                {
                    return Results.BadRequest(new { error = result.Error });
                }
                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("LeaveChat")
        .WithDescription("Removes the current user from a chat group")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
