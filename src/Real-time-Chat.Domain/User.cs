using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Real_time_Chat.Domain;

public class User : IdentityUser<Guid>
{
    public string AvatarPath { get; set; } = string.Empty;
    
    [NotMapped]
    public bool IsOnline { get; set; }
}
