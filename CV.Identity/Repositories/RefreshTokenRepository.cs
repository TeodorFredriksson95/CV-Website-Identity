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

        public async Task<string> GenerateAndStoreRefreshToken(string userId, string refreshToken, DateTime expiryDate)
        {

            var sql = @"INSERT INTO refresh_tokens (user_id, token, expires_at, created_at, revoked)
                VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt, false)";
            using var dbConnection = await _connectionFactory.CreateConnectionAsync();

            await dbConnection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = expiryDate,
                CreatedAt = DateTime.UtcNow
            });

            return refreshToken;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var sql = @"SELECT * FROM refresh_tokens WHERE token = @Token";
            return await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }
        public async Task<bool> UpdateRefreshTokenAsync(RefreshToken refreshToken, IDbConnection dbConnection)
        {
            var sql = @"UPDATE refresh_tokens SET expires_at = @ExpiresAt, revoked = @Revoked WHERE token = @Token";
            var updated = await dbConnection.ExecuteAsync(sql, refreshToken);
            return updated > 0;
        }
    }
}
