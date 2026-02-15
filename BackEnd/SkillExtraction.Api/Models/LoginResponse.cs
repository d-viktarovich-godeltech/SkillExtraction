namespace SkillExtraction.Api.Models;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}
