using MediatR;
using SkillExtraction.Core.Commands;
using SkillExtraction.Core.Interfaces;

namespace SkillExtraction.Core.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Try to find user by username or email
        var user = await _userRepository.GetUserByUsernameAsync(request.UsernameOrEmail);
        if (user == null)
        {
            user = await _userRepository.GetUserByEmailAsync(request.UsernameOrEmail);
        }

        if (user == null)
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Invalid username/email or password"
            };
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Invalid username/email or password"
            };
        }

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResult
        {
            Success = true,
            User = user,
            Token = token
        };
    }
}
