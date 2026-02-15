using MediatR;
using SkillExtraction.Core.Commands;
using SkillExtraction.Core.Interfaces;

namespace SkillExtraction.Core.Handlers;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterUserHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if username already exists
        var existingUser = await _userRepository.GetUserByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Check if email already exists
        existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = await _userRepository.CreateUserAsync(request.Username, request.Email, passwordHash);

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(user);

        return new RegisterUserResult
        {
            User = user,
            Token = token
        };
    }
}
