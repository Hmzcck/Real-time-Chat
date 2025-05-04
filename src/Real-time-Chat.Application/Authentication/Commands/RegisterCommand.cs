using MediatR;
using Microsoft.AspNetCore.Identity;
using Real_time_Chat.Application.Interfaces;
using Real_time_Chat.Application.Services;
using Real_time_Chat.Domain.Entities;

namespace Real_time_Chat.Application.Authentication.Commands
{
    public sealed record RegiserCommand : IRequest<RegisterCommandResponse>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    };

    public sealed record RegisterCommandResponse
    {
        public Guid Id { get; init; } = default!;
        public string Email { get; init; } = string.Empty;
        public string AvatarPath { get; init; } = string.Empty;
    }
    
    internal sealed class RegisterCommandHanler(UserManager<User> userManager,
        IJwtService jwtService, IApplicationDbContext applicationDbContext) : IRequestHandler<RegiserCommand, RegisterCommandResponse>
    {
        public async Task<RegisterCommandResponse> Handle(RegiserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                throw new Exception("User already exists");
            }

            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                AvatarPath = "default.png"
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new Exception("User creation failed");
            }

            return new RegisterCommandResponse
            {
                Id = user.Id,
                Email = user.Email,
                AvatarPath = user.AvatarPath
            };
        }
    }
}
