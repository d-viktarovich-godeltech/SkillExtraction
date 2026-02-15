using SkillExtraction.Core.Models;

namespace SkillExtraction.Core.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
