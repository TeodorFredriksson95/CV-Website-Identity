using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV.Identity.Database
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default);
    }

    public class NpgsqlConnectionFactory: IDbConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        public NpgsqlConnectionFactory(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;   
        }

        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default)
        {
            try
            {
                _logger.LogInformation(_connectionString);
                var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(token);
                Console.WriteLine("Connection opened successfully.");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening database connection: {ex.Message}");
                throw;
            }
        }
    }
}
