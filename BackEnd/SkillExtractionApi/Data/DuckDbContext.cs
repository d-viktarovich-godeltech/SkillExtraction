using DuckDB.NET.Data;
using SkillExtractionApi.Models;
using System.Data;

namespace SkillExtractionApi.Data;

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
                FilePath VARCHAR NOT NULL,
                UploadDate TIMESTAMP NOT NULL,
                FileSize BIGINT NOT NULL,
                ExtractedSkills VARCHAR,
                OpenAiResponse VARCHAR,
                ProcessingStatus VARCHAR NOT NULL,
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

    // User operations
    public async Task<User?> GetUserByIdAsync(int id)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE Id = $1";
        command.Parameters.Add(new DuckDBParameter(id));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }
        return null;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE Username = $1";
        command.Parameters.Add(new DuckDBParameter(username));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }
        return null;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE Email = $1";
        command.Parameters.Add(new DuckDBParameter(email));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }
        return null;
    }

    public async Task<User> CreateUserAsync(string username, string email, string passwordHash)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Users (Id, Username, Email, PasswordHash, CreatedAt)
            VALUES (nextval('users_id_seq'), $1, $2, $3, $4)
            RETURNING *";
        
        command.Parameters.Add(new DuckDBParameter(username));
        command.Parameters.Add(new DuckDBParameter(email));
        command.Parameters.Add(new DuckDBParameter(passwordHash));
        command.Parameters.Add(new DuckDBParameter(DateTime.UtcNow));

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        
        return new User
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            CreatedAt = reader.GetDateTime(4)
        };
    }

    // CV Upload operations
    public async Task<CvUpload> CreateCvUploadAsync(CvUpload upload)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO CvUploads (Id, UserId, FileName, FilePath, UploadDate, FileSize, 
                                    ExtractedSkills, OpenAiResponse, ProcessingStatus)
            VALUES (nextval('cvuploads_id_seq'), $1, $2, $3, $4, $5, $6, $7, $8)
            RETURNING *";
        
        command.Parameters.Add(new DuckDBParameter(upload.UserId));
        command.Parameters.Add(new DuckDBParameter(upload.FileName));
        command.Parameters.Add(new DuckDBParameter(upload.FilePath));
        command.Parameters.Add(new DuckDBParameter(upload.UploadDate));
        command.Parameters.Add(new DuckDBParameter(upload.FileSize));
        command.Parameters.Add(new DuckDBParameter(upload.ExtractedSkills));
        command.Parameters.Add(new DuckDBParameter(upload.OpenAiResponse));
        command.Parameters.Add(new DuckDBParameter(upload.ProcessingStatus));

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        
        return ReadCvUploadFromReader(reader);
    }

    public async Task<List<CvUpload>> GetUserCvUploadsAsync(int userId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM CvUploads WHERE UserId = $1 ORDER BY UploadDate DESC";
        command.Parameters.Add(new DuckDBParameter(userId));

        var uploads = new List<CvUpload>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            uploads.Add(ReadCvUploadFromReader(reader));
        }
        return uploads;
    }

    public async Task<CvUpload?> GetCvUploadByIdAsync(int id, int userId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM CvUploads WHERE Id = $1 AND UserId = $2";
        command.Parameters.Add(new DuckDBParameter(id));
        command.Parameters.Add(new DuckDBParameter(userId));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadCvUploadFromReader(reader);
        }
        return null;
    }

    public async Task UpdateCvUploadAsync(CvUpload upload)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CvUploads 
            SET ExtractedSkills = $1, OpenAiResponse = $2, ProcessingStatus = $3
            WHERE Id = $4";
        
        command.Parameters.Add(new DuckDBParameter(upload.ExtractedSkills));
        command.Parameters.Add(new DuckDBParameter(upload.OpenAiResponse));
        command.Parameters.Add(new DuckDBParameter(upload.ProcessingStatus));
        command.Parameters.Add(new DuckDBParameter(upload.Id));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> DeleteCvUploadAsync(int id, int userId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM CvUploads WHERE Id = $1 AND UserId = $2";
        command.Parameters.Add(new DuckDBParameter(id));
        command.Parameters.Add(new DuckDBParameter(userId));

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static CvUpload ReadCvUploadFromReader(IDataReader reader)
    {
        return new CvUpload
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            FileName = reader.GetString(2),
            FilePath = reader.GetString(3),
            UploadDate = reader.GetDateTime(4),
            FileSize = reader.GetInt64(5),
            ExtractedSkills = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            OpenAiResponse = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
            ProcessingStatus = reader.GetString(8)
        };
    }
}
