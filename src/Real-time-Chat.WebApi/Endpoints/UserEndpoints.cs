using MediatR;
using Microsoft.AspNetCore.Authorization;
using Real_time_Chat.Application.Features.Users;

namespace Real_time_Chat.WebApi.Endpoints;

[Authorize]
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        // GET /api/users?searchTerm=ahmet&pageSize=10&pageNumber=1
        group.MapGet("/", async (
            string? searchTerm,
            int? pageSize,
            int? pageNumber,
            IMediator mediator) =>
        {
            try
            {
                var query = new GetUsersQuery(
                    SearchTerm: searchTerm,
                    PageSize: pageSize ?? 10,
                    PageNumber: pageNumber ?? 1
                );

                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetUsers")
        .WithDescription("Returns a list of users, optionally filtered by search term")
        .Produces<List<GetUsersQueryReponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
