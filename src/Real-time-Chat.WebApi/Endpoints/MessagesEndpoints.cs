using MediatR;
using Microsoft.AspNetCore.Authorization;
using Real_time_Chat.Application.Features.Messages.Commands;
using Real_time_Chat.Application.Features.Messages.Queries;

namespace Real_time_Chat.WebApi.Endpoints;

[Authorize]
public static class MessagesEndpoints
{
    public static void MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/messages")
            .WithTags("Messages")
            .WithOpenApi();

        // Get messages for a chat
        group.MapGet("/chat/{chatId}", async (Guid chatId, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetMessagesQuery(chatId));
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetMessages")
        .WithDescription("Returns all messages for a specific chat")
        .Produces<List<GetMessageQueryResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // Send a message
        group.MapPost("/", async (SendMessageCommand request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(request);
                return Results.Created($"/api/messages/{result.Id}", result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
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
        .WithName("SendMessage")
        .WithDescription("Sends a new message to a chat")
        .Produces<SendMessageCommandResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
