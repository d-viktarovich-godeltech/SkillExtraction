using Mapster;
using SkillExtraction.Api.Models;
using SkillExtraction.Core.Models;

namespace SkillExtraction.Api.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        // User to UserResponse
        TypeAdapterConfig<User, UserResponse>.NewConfig();

        // CvUpload to CvUploadResponse
        TypeAdapterConfig<CvUpload, CvUploadResponse>.NewConfig();
    }
}
