using MediatR;
using Real_time_Chat.Application.Authentication.Commands;

namespace Real_time_Chat.WebApi.Endpoints;

public static class AuthenticationEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // Login endpoint
        group.MapPost("/login", async (LoginCommand request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(request);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {

                return Results.Problem(
                    detail: ex.Message,
                    title: "Authentication failed",
                    statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("Login")
        .WithDescription("Authenticates a user and returns a JWT token")
        .Produces<LoginCommandResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // Register endpoint
        group.MapPost("/register", async (RegiserCommand request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(request);
                return Results.Created($"/api/users/{result.Id}", result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("Register")
        .WithDescription("Registers a new user")
        .Produces<RegisterCommandResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
