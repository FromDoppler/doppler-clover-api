using System.Threading.Tasks;
using Dapper;
using Doppler.CloverAPI.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Doppler.CloverAPI.Services
{
    public class ClientAddressService : IClientAddressService
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientAddressService(IDatabaseConnectionFactory connectionFactory, IHttpContextAccessor httpContextAccessor)
        {
            _connectionFactory = connectionFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetIpAddress(string email)
        {
            using var connection = _connectionFactory.GetConnection();

            var clientIp = await connection.QuerySingleOrDefaultAsync<string>(@"SELECT [RegistrationIp]
FROM [dbo].[User]
WHERE [dbo].[User].[Email] = @email", new { email });

            clientIp ??= _httpContextAccessor.HttpContext?.Request.Headers["Cf-Connecting-Ip"];

            return clientIp;
        }
    }
}
