using DuckDB.NET.Data;
using Mapster;
using SkillExtraction.Core.Interfaces;
using SkillExtraction.Core.Models;
using SkillExtraction.Data.Entities;
using System.Text.Json;

namespace SkillExtraction.Data.Repositories;

public class CvRepository : ICvRepository
{
    private readonly DuckDbContext _context;

    public CvRepository(DuckDbContext context)
    {
        _context = context;
    }

    public async Task<CvUpload> CreateCvUploadAsync(CvUpload cvUpload)
    {
        using var connection = _context.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO CvUploads (Id, UserId, FileName, StoragePath, UploadDate, FileSize, 
                                    ExtractedSkills, Summary, AnalysisResult)
            VALUES (nextval('cvuploads_id_seq'), $1, $2, $3, $4, $5, $6, $7, $8)
            RETURNING *";
        
        command.Parameters.Add(new DuckDBParameter(cvUpload.UserId));
        command.Parameters.Add(new DuckDBParameter(cvUpload.FileName));
        command.Parameters.Add(new DuckDBParameter(cvUpload.StoragePath));
        command.Parameters.Add(new DuckDBParameter(DateTime.UtcNow));
        command.Parameters.Add(new DuckDBParameter(cvUpload.FileSize));
        command.Parameters.Add(new DuckDBParameter(JsonSerializer.Serialize(cvUpload.ExtractedSkills)));
        command.Parameters.Add(new DuckDBParameter(cvUpload.Summary));
        command.Parameters.Add(new DuckDBParameter(cvUpload.AnalysisResult));

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        
        var entity = ReadCvUploadEntity(reader);
        return entity.Adapt<CvUpload>();
    }

    public async Task<CvUpload?> GetCvByIdAsync(int id, int userId)
    {
        using var connection = _context.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM CvUploads WHERE Id = $1 AND UserId = $2";
        command.Parameters.Add(new DuckDBParameter(id));
        command.Parameters.Add(new DuckDBParameter(userId));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var entity = ReadCvUploadEntity(reader);
            return entity.Adapt<CvUpload>();
        }
        return null;
    }

    public async Task<List<CvUpload>> GetUserCvHistoryAsync(int userId, int limit = 10)
    {
        using var connection = _context.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM CvUploads WHERE UserId = $1 ORDER BY UploadDate DESC LIMIT $2";
        command.Parameters.Add(new DuckDBParameter(userId));
        command.Parameters.Add(new DuckDBParameter(limit));

        var uploads = new List<CvUpload>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var entity = ReadCvUploadEntity(reader);
            uploads.Add(entity.Adapt<CvUpload>());
        }
        return uploads;
    }

    public async Task<bool> DeleteCvAsync(int id, int userId)
    {
        using var connection = _context.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM CvUploads WHERE Id = $1 AND UserId = $2";
        command.Parameters.Add(new DuckDBParameter(id));
        command.Parameters.Add(new DuckDBParameter(userId));

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static CvUploadEntity ReadCvUploadEntity(System.Data.IDataReader reader)
    {
        var extractedSkillsJson = reader.IsDBNull(6) ? "[]" : reader.GetString(6);
        var skills = JsonSerializer.Deserialize<List<string>>(extractedSkillsJson) ?? new List<string>();

        return new CvUploadEntity
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            FileName = reader.GetString(2),
            StoragePath = reader.GetString(3),
            UploadDate = reader.GetDateTime(4),
            FileSize = reader.GetInt64(5),
            ExtractedSkills = extractedSkillsJson,
            Summary = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
            AnalysisResult = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
        };
    }
}
