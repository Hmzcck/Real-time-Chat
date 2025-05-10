using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Real_time_Chat.Domain.Entities;
using System.IO;

namespace Real_time_Chat.Application.Authentication.Commands
{
    public sealed record RegiserCommand : IRequest<RegisterCommandResponse>
    {
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public IFormFile? AvatarFile { get; init; }
    };

    public sealed record RegisterCommandResponse
    {
        public Guid Id { get; init; } = Guid.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string AvatarPath { get; init; } = string.Empty;
    }

    internal sealed class RegisterCommandHandler(UserManager<User> userManager) : IRequestHandler<RegiserCommand, RegisterCommandResponse>
    {
        public async Task<RegisterCommandResponse> Handle(RegiserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                throw new Exception("User already exists");
            }

            string avatarFileName = "default.png";
            if (request.AvatarFile is not null)
            {
                // Generate unique filename
                string fileExtension = Path.GetExtension(request.AvatarFile.FileName);
                avatarFileName = $"{Guid.NewGuid()}{fileExtension}";
                
                // Ensure avatars directory exists
                string avatarsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                Directory.CreateDirectory(avatarsPath);

                // Save the file
                string filePath = Path.Combine(avatarsPath, avatarFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await request.AvatarFile.CopyToAsync(stream);
            }

            var user = new User
            {
                Email = request.Email,
                UserName = request.UserName,
                AvatarPath = avatarFileName
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
                UserName = user.UserName,
                AvatarPath = user.AvatarPath
            };
        }
    }
}
