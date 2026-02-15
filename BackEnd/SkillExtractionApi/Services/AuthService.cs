using Microsoft.IdentityModel.Tokens;
using SkillExtractionApi.Data;
using SkillExtractionApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SkillExtractionApi.Services;

public class AuthService
{
    private readonly DuckDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(DuckDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<User> RegisterUserAsync(string username, string email, string password)
    {
        // Check if username already exists
        var existingUser = await _dbContext.GetUserByUsernameAsync(username);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Check if email already exists
        existingUser = await _dbContext.GetUserByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Create user
        return await _dbContext.CreateUserAsync(username, email, passwordHash);
    }

    public async Task<User?> ValidateUserAsync(string usernameOrEmail, string password)
    {
        // Try to find user by username or email
        var user = await _dbContext.GetUserByUsernameAsync(usernameOrEmail);
        if (user == null)
        {
            user = await _dbContext.GetUserByEmailAsync(usernameOrEmail);
        }

        if (user == null)
        {
            return null;
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["JWT:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret not configured");
        var jwtIssuer = _configuration["JWT:Issuer"] 
            ?? throw new InvalidOperationException("JWT Issuer not configured");
        var jwtAudience = _configuration["JWT:Audience"] 
            ?? throw new InvalidOperationException("JWT Audience not configured");
        var expirationMinutes = int.Parse(_configuration["JWT:ExpirationMinutes"] ?? "1440");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
