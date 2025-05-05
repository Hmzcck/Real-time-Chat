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
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("Login")
        .WithDescription("Authenticates a user and returns a JWT token");

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
        .WithDescription("Registers a new user");
    }
}