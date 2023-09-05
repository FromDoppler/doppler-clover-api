using System.Threading.Tasks;
using Dapper;

namespace Doppler.CloverAPI.Infrastructure
{
    public class LoginXUserRepository : ILoginXUserRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public LoginXUserRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<string> GetIpAddressOfLastLoginByUserId(int userId)
        {
            using var connection = _connectionFactory.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<string>(@"SELECT TOP 1
[IpAddress]
FROM [Doppler2011].[dbo].[LoginXUser]
WHERE [LoginXUser].IdUser = @userId
ORDER BY [LoginXUser].LoginDate DESC;", new { userId });
        }
    }
}
