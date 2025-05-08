using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Real_time_Chat.Application.Interfaces;
using Real_time_Chat.Domain.Entities;

namespace Real_time_Chat.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor,
UserManager<User> userManager) : ICurrentUserService
{
    public Guid UserId => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true ? Guid.Parse(userManager.GetUserId(httpContextAccessor.HttpContext.User)) : Guid.Empty;
}
