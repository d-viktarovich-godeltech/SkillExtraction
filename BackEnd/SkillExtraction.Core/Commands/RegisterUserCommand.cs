using MediatR;
using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Commands;

public class RegisterUserCommand : IRequest<RegisterUserResult>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterUserResult
{
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}
