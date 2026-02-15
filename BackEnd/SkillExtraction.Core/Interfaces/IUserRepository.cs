using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Interfaces;

public interface IUserRepository
{
    Task<User> CreateUserAsync(string username, string email, string passwordHash);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
}
