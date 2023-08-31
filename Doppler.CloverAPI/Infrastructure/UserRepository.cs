using System.Threading.Tasks;
using Dapper;

namespace Doppler.CloverAPI.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public UserRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<string> GetIpAddressByEmail(string email)
        {
            using var connection = _connectionFactory.GetConnection();

            return await connection.QuerySingleOrDefaultAsync<string>(@"SELECT [RegistrationIp]
FROM [dbo].[User]
WHERE [dbo].[User].[Email] = @email", new { email });
        }
    }
}
