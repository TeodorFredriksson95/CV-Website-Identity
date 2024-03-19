using CV.Identity.Database;
using CV.Identity.Models;using System.Data;

using Microsoft.AspNetCore.Connections;
using Dapper;
using Newtonsoft.Json.Linq;

namespace CV.Identity.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public UserRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _connectionFactory = dbConnectionFactory;
        }
        public async Task CreateUser(User user)
        {
            const string command = @"
            INSERT INTO users (user_id, firstname, lastname, email, country_id)
            VALUES (@UserId, @FirstName, @LastName, @Email, @CountryId)"
            ;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(command, user);
        }

        public async Task<User> GetUser(string userId)
        {
            const string query = "SELECT" +
                " user_id AS UserId," +
                "firstname AS FirstName," +
                "lastname AS LastName," +
                "email AS Email," +
                "country_id AS CountryId" +
                " FROM users WHERE user_id = @UserId";

            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.QuerySingleOrDefaultAsync<User>(query, new { UserId = userId });
        }

        public async Task<bool> UserExists(string userId)
        {
            const string query = "SELECT COUNT(1) FROM users WHERE user_id = @UserId";

            using var connection = await _connectionFactory.CreateConnectionAsync();
            var exists = await connection.ExecuteScalarAsync<bool>(query, new { UserId = userId });
            return exists;
        }
    }
}
