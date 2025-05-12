using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Domain.Entities;
using Real_time_Chat.Infrastructure.Persistence.Contexts;
using Real_time_Chat.Infrastructure.Persistence.Seeders;

namespace Real_time_Chat.WebApi.Endpoints;

public static class SeedEndpoints
{
    public static IEndpointRouteBuilder MapSeedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/seed");

        group.MapPost("/", async (DataSeeder seeder) =>
        {
            try
            {
                var result = await seeder.SeedAsync();
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Failed to seed data: {ex.Message}" });
            }
        })
        .WithName("SeedData")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Seeds the database with fake data",
            Description = "Creates sample users, chats, and messages using Bogus fake data generator. Returns the created users and groups."
        });

        group.MapDelete("/", async (ApplicationDbContext context, UserManager<User> userManager) =>
        {
            try
            {
                // Delete all messages
                await context.Messages.ExecuteDeleteAsync();
                
                // Delete all UserChats
                await context.UserChats.ExecuteDeleteAsync();
                
                // Delete all chats
                await context.Chats.ExecuteDeleteAsync();
                
                // Get all users and delete them through UserManager
                var users = await userManager.Users.ToListAsync();
                foreach (var user in users)
                {
                    await userManager.DeleteAsync(user);
                }
                
                return Results.Ok(new { message = "All data deleted successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"Failed to delete data: {ex.Message}" });
            }
        })
        .WithName("DeleteData")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Deletes all data from the database",
            Description = "Removes all messages, chats, and users from the database"
        });

        return app;
    }
}
