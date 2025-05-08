using MediatR;
using Microsoft.AspNetCore.Identity;
using Real_time_Chat.Application.Services;
using Real_time_Chat.Domain.Entities;

namespace Real_time_Chat.Application.Authentication.Commands;

public sealed record LoginCommand(
    string UserNameOrEmail,
    string Password
) : IRequest<LoginCommandResponse>;


public sealed record LoginCommandResponse
{
    public Guid UserId { get; init; }
    public string Token { get; init; } = string.Empty;
    public string AvatarPath { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
}


internal sealed class LoginCommandHandler(UserManager<User> userManager, IJwtService jwtService,
SignInManager<User> signInManager) : IRequestHandler<LoginCommand, LoginCommandResponse>
{

    public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.UserNameOrEmail);

        if (user is null)
        {
            user = await userManager.FindByNameAsync(request.UserNameOrEmail);
        }

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var result = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true
        );

        if (result.IsLockedOut)
        {
            throw new UnauthorizedAccessException("Account is locked. Please try again later.");
        }

        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = await jwtService.CreateTokenAsync(user, cancellationToken);

        return new LoginCommandResponse
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Token = token,
            AvatarPath = user.AvatarPath,
            Email = user.Email!
        };
    }
}