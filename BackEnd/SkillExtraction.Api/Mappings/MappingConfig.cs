using Mapster;
using SkillExtraction.Api.DTOs;
using SkillExtraction.Core.Models;

namespace SkillExtraction.Api.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        // User to UserDto
        TypeAdapterConfig<User, UserDto>.NewConfig();

        // CvUpload to CvUploadResponse
        TypeAdapterConfig<CvUpload, CvUploadResponse>.NewConfig();
    }
}
