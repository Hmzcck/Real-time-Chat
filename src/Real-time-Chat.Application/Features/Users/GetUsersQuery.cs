using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Real_time_Chat.Domain.Entities;
namespace Real_time_Chat.Application.Features.Users;

public sealed record GetUsersQuery
(
    string? SearchTerm,
    int PageSize = 10,
    int PageNumber = 1
) : IRequest<List<GetUsersQueryReponse>>;

public sealed record GetUsersQueryReponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarPath { get; set; } = string.Empty;
}

internal sealed class GetUsersQueryHandler(UserManager<User> userManager) : IRequestHandler<GetUsersQuery, List<GetUsersQueryReponse>>
{
    public async Task<List<GetUsersQueryReponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .Where(x =>
                (request.SearchTerm == null) ||
                x.UserName!.Contains(request.SearchTerm) ||
                x.Email!.Contains(request.SearchTerm))
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return users.Select(u => new GetUsersQueryReponse
        {
            Id = u.Id,
            UserName = u.UserName!,
            Email = u.Email!,
            AvatarPath = u.AvatarPath ?? string.Empty
        }).ToList();
    }
}