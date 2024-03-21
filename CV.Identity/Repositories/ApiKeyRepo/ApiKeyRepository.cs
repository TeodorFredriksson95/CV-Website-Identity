using CV.Identity.Database;
using CV.Identity.Models;
using Dapper;
using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Data;

namespace CV.Identity.Repositories.ApiKeyRepo
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public ApiKeyRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _connectionFactory = dbConnectionFactory;
        }

        public async Task<ApiToken> GetByApiKeyAsync(string apiKey)
        {
            const string sql = @"
            SELECT 
                api_key_id AS ApiKeyId, 
                user_id AS UserId, 
                api_key AS ApiKey, 
                expires_at AS ExpiresAt, 
                created_at AS CreatedAt, 
                revoked AS Revoked, 
                replaced_by_api_key AS ReplacedByApiKey
            FROM api_keys
            WHERE api_key=@ApiKey";
            using var dbConnection = await _connectionFactory.CreateConnectionAsync();
            return await dbConnection.QuerySingleOrDefaultAsync<ApiToken>(sql, new { ApiKey = apiKey});
        }

        public async Task<ApiToken> GetUserLastActiveApiKeyAsync(string userId)
        {
            const string sql = @"
            SELECT 
                api_key_id AS ApiKeyId, 
                user_id AS UserId, 
                api_key AS ApiKey, 
                expires_at AS ExpiresAt, 
                created_at AS CreatedAt, 
                revoked AS Revoked, 
                replaced_by_api_key AS ReplacedByApiKey
            FROM api_keys
            WHERE user_id=@UserId AND revoked=false";

            using var dbConnection = await _connectionFactory.CreateConnectionAsync();
            return await dbConnection.QuerySingleOrDefaultAsync<ApiToken>(sql, new { UserId = userId });
        }

        public async Task RevokeApiKey(string oldApiKey, string newApiKey = null)
        {
            const string sql = @"UPDATE api_keys SET revoked = TRUE, replaced_by_api_key = @ReplacedByApiKey WHERE api_key = @ApiKey";
            using var dbConnection = await _connectionFactory.CreateConnectionAsync();
            await dbConnection.ExecuteReaderAsync(sql, new { ApiKey = oldApiKey, ReplacedByApiKey = newApiKey });
            
        }

        public async Task StoreApiKey(string userId, string apiKey, DateTime expiresAt)
        {
            const string sql = @"
            INSERT INTO api_keys (user_id, api_key, expires_at, created_at, revoked)
            VALUES (@UserId, @ApiKey, @ExpiresAt, @CreatedAt, @Revoked)";

            using var dbConnection = await _connectionFactory.CreateConnectionAsync();

            try
            {

            await dbConnection.ExecuteAsync(sql, new
            {
                UserId = userId,
                ApiKey = apiKey,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                Revoked = false
            });
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine($"Detail: {ex}"); // Detail is specific to NpgsqlException
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }

        }

    }
}
