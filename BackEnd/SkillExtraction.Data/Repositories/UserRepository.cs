using DuckDB.NET.Data;
using Mapster;
using SkillExtraction.Core.Interfaces;
using SkillExtraction.Core.Models;
using SkillExtraction.Data.Entities;

namespace SkillExtraction.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DuckDbContext _context;

    public UserRepository(DuckDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateUserAsync(string username, string email, string passwordHash)
    {
        using var connection = _context.CreateConnection();
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
        
        var entity = new UserEntity
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            CreatedAt = reader.GetDateTime(4)
        };

        return entity.Adapt<User>();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE Id = $1";
        command.Parameters.Add(new DuckDBParameter(id));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var entity = new UserEntity
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
            return entity.Adapt<User>();
        }
        return null;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = _context.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE Username = $1";
        command.Parameters.Add(new DuckDBParameter(username));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var entity = new UserEntity
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
            return entity.Adapt<User>();
        }
        return null;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = _context.CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE Email = $1";
        command.Parameters.Add(new DuckDBParameter(email));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var entity = new UserEntity
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
            return entity.Adapt<User>();
        }
        return null;
    }
}
