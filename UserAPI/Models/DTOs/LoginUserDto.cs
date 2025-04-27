using System.ComponentModel.DataAnnotations;

namespace UserAPI.Models.DTOs;

public class LoginUserDto
{
    [Microsoft.Build.Framework.Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
    
    [Microsoft.Build.Framework.Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}