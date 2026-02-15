using Mapster;
using SkillExtraction.Core.Models;
using SkillExtraction.Data.Entities;
using System.Text.Json;

namespace SkillExtraction.Data.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        // User mappings
        TypeAdapterConfig<UserEntity, User>.NewConfig();
        TypeAdapterConfig<User, UserEntity>.NewConfig();

        // CvUpload mappings
        TypeAdapterConfig<CvUploadEntity, CvUpload>.NewConfig()
            .Map(dest => dest.ExtractedSkills, 
                 src => DeserializeSkills(src.ExtractedSkills));

        TypeAdapterConfig<CvUpload, CvUploadEntity>.NewConfig()
            .Map(dest => dest.ExtractedSkills, 
                 src => SerializeSkills(src.ExtractedSkills));
    }

    private static List<string> DeserializeSkills(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<string>();
        
        return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }

    private static string SerializeSkills(List<string> skills)
    {
        return JsonSerializer.Serialize(skills);
    }
}
