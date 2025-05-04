using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Real_time_Chat.Application.Services;
using Real_time_Chat.Domain.Entities;
using Real_time_Chat.Infrastructure.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Real_time_Chat.Infrastructure.Services;

public class JwtService(IOptions<JwtSettings> _jwtSettings, UserManager<User> _userManager) : IJwtService
{
    public async Task<string> CreateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? throw new InvalidOperationException("User email is required for token creation")),
        };

        var expires = DateTime.Now.AddDays(1);

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtSettings.Value.Secret));
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha512);

        JwtSecurityToken securityToken = new(
            issuer: _jwtSettings.Value.Issuer,
            audience: _jwtSettings.Value.Audience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: expires,
            signingCredentials: signingCredentials);

        JwtSecurityTokenHandler handler = new();

        var token = handler.WriteToken(securityToken);

        return token;
    }
}