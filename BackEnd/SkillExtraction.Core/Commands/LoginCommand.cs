using MediatR;
using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Commands;

public class LoginCommand : IRequest<LoginResult>
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResult
{
    public bool Success { get; set; }
    public User? User { get; set; }
    public string Token { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
