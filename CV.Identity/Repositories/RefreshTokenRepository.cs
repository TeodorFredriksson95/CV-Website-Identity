using CV.Identity.Database;
using CV.Identity.Models;
using CV.Identity.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV.Application.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<RefreshToken> GenerateAndStoreRefreshToken(RefreshToken refreshToken)
        {

            var sql = @"INSERT INTO refresh_tokens (user_id, token, expires_at, created_at, revoked)
                VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt, false)";
            using var dbConnection = await _connectionFactory.CreateConnectionAsync();

            await dbConnection.ExecuteAsync(sql, refreshToken);

            return refreshToken;
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            const string sql = @"
            SELECT 
                refresh_token_id AS RefreshTokenId, 
                user_id AS UserId, 
                token AS Token, 
                expires_at AS ExpiresAt, 
                created_at AS CreatedAt, 
                revoked AS Revoked, 
                replaced_by_token AS ReplacedByToken
            FROM refresh_tokens 
            WHERE token=@Token";
            using var dbConnection = await _connectionFactory.CreateConnectionAsync();
            return await dbConnection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { Token = token });


        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var sql = @"SELECT * FROM refresh_tokens WHERE token = @Token";
            return await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task RevokeRefreshTokenAsync(string token, string replacedByToken = null)
        {
            const string sql = @"UPDATE refresh_tokens SET revoked = TRUE, replaced_by_token = @ReplacedByToken WHERE token = @Token";
            using var dbConnection = await _connectionFactory.CreateConnectionAsync();
            await dbConnection.ExecuteReaderAsync(sql, new {Token = token, ReplacedByToken = replacedByToken});
        }

        public async Task StoreRefreshToken(string userId, string token, DateTime expiresAt)
        {
            const string sql = @"
            INSERT INTO refresh_tokens (user_id, token, expires_at, created_at, revoked)
            VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt, @Revoked)";

            using var dbConnection = await _connectionFactory.CreateConnectionAsync();
            await dbConnection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Token = token,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                Revoked = false
            });
        }

        public async Task<bool> UpdateRefreshTokenAsync(RefreshToken refreshToken, IDbConnection dbConnection)
        {
            var sql = @"UPDATE refresh_tokens SET expires_at = @ExpiresAt, revoked = @Revoked WHERE token = @Token";
            var updated = await dbConnection.ExecuteAsync(sql, refreshToken);
            return updated > 0;
        }
    }
}
