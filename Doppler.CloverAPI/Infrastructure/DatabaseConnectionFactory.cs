using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Doppler.CloverAPI.Infrastructure
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string _connectionString;

        public DatabaseConnectionFactory(IOptions<DopplerDatabaseSettings> dopplerDataBaseSettings)
        {
            _connectionString = dopplerDataBaseSettings.Value.GetSqlConnectionString();
        }

        public IDbConnection GetConnection() => new SqlConnection(_connectionString);
    }
}
