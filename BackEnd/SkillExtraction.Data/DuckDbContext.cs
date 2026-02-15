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

        // Migrate old schema if needed
        await MigrateSchemaAsync(connection);
    }

    private async Task MigrateSchemaAsync(DuckDBConnection connection)
    {
        try
        {
            // Check if we need to migrate from FilePath to StoragePath
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name = 'CvUploads' AND column_name = 'FilePath'";
            var hasFilePath = await checkCommand.ExecuteScalarAsync();

            if (hasFilePath != null)
            {
                // Rename FilePath to StoragePath
                using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = "ALTER TABLE CvUploads RENAME COLUMN FilePath TO StoragePath";
                await alterCommand.ExecuteNonQueryAsync();
            }

            // Remove old ProcessingStatus column if it exists
            checkCommand.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name = 'CvUploads' AND column_name = 'ProcessingStatus'";
            var hasProcessingStatus = await checkCommand.ExecuteScalarAsync();

            if (hasProcessingStatus != null)
            {
                using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = "ALTER TABLE CvUploads DROP COLUMN ProcessingStatus";
                await alterCommand.ExecuteNonQueryAsync();
            }

            // Rename OpenAiResponse to AnalysisResult if it exists
            checkCommand.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name = 'CvUploads' AND column_name = 'OpenAiResponse'";
            var hasOpenAiResponse = await checkCommand.ExecuteScalarAsync();

            if (hasOpenAiResponse != null)
            {
                using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = "ALTER TABLE CvUploads RENAME COLUMN OpenAiResponse TO AnalysisResult";
                await alterCommand.ExecuteNonQueryAsync();
            }

            // Check if we need to add Summary column
            checkCommand.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name = 'CvUploads' AND column_name = 'Summary'";
            var hasSummary = await checkCommand.ExecuteScalarAsync();

            if (hasSummary == null)
            {
                using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = "ALTER TABLE CvUploads ADD COLUMN Summary VARCHAR";
                await alterCommand.ExecuteNonQueryAsync();
            }

            // Check if we need to add AnalysisResult column (if OpenAiResponse didn't exist)
            checkCommand.CommandText = "SELECT column_name FROM information_schema.columns WHERE table_name = 'CvUploads' AND column_name = 'AnalysisResult'";
            var hasAnalysisResult = await checkCommand.ExecuteScalarAsync();

            if (hasAnalysisResult == null)
            {
                using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = "ALTER TABLE CvUploads ADD COLUMN AnalysisResult VARCHAR";
                await alterCommand.ExecuteNonQueryAsync();
            }
        }
        catch
        {
            // Ignore migration errors if table doesn't exist yet
        }
    }
}
