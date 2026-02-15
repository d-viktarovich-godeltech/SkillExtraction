using DuckDB.NET.Data;
using Microsoft.Extensions.Configuration;

namespace SkillExtraction.Data;

public class DuckDbContext
{
    private readonly string _connectionString;

    public DuckDbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DuckDB") 
            ?? throw new ArgumentNullException("DuckDB connection string not configured");
    }

    public DuckDBConnection CreateConnection()
    {
        return new DuckDBConnection(_connectionString);
    }

    public async Task InitializeDatabaseAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        // Create Users table
        var createUsersTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY,
                Username VARCHAR NOT NULL UNIQUE,
                Email VARCHAR NOT NULL UNIQUE,
                PasswordHash VARCHAR NOT NULL,
                CreatedAt TIMESTAMP NOT NULL
            )";

        // Create CvUploads table
        var createCvUploadsTable = @"
            CREATE TABLE IF NOT EXISTS CvUploads (
                Id INTEGER PRIMARY KEY,
                UserId INTEGER NOT NULL,
                FileName VARCHAR NOT NULL,
                StoragePath VARCHAR NOT NULL,
                UploadDate TIMESTAMP NOT NULL,
                FileSize BIGINT NOT NULL,
                ExtractedSkills VARCHAR,
                Summary VARCHAR,
                AnalysisResult VARCHAR,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            )";

        // Create sequence for auto-increment
        var createUsersSequence = "CREATE SEQUENCE IF NOT EXISTS users_id_seq START 1";
        var createCvUploadsSequence = "CREATE SEQUENCE IF NOT EXISTS cvuploads_id_seq START 1";

        using var command = connection.CreateCommand();
        
        command.CommandText = createUsersSequence;
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = createUsersTable;
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = createCvUploadsSequence;
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = createCvUploadsTable;
        await command.ExecuteNonQueryAsync();
    }
}
