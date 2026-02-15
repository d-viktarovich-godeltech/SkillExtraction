using System.ComponentModel.DataAnnotations;

namespace SkillExtraction.Api.Models;

public class LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
