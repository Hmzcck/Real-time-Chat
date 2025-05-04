using MediatR;
using Microsoft.AspNetCore.Identity;
using Real_time_Chat.Application.Services;
using Real_time_Chat.Domain.Entities;

namespace Real_time_Chat.Application.Authentication.Commands;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginCommandResponse>;


public sealed record LoginCommandResponse
{
    public string Token { get; init; } = default!;
    public string AvatarPath { get; init; } = default!;
    public string Email { get; init; } = default!;
}


internal sealed class LoginCommandHandler(UserManager<User> userManager,
IJwtService jwtService,
SignInManager<User> signInManager) : IRequestHandler<LoginCommand, LoginCommandResponse>
{

       public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
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

        var token = await jwtService.CreateTokenAsync(user);

        return new LoginCommandResponse
        {
            Token = token,
            AvatarPath = user.AvatarPath,
            Email = user.Email!
        };
    }
}