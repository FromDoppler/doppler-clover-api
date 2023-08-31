using System.Threading.Tasks;
using Dapper;
using Doppler.CloverAPI.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Doppler.CloverAPI.Services
{
    public class ClientAddressService : IClientAddressService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientAddressService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetIpAddress(string email)
        {
            var clientIp = await _userRepository.GetIpAddressByEmail(email);

            if (string.IsNullOrEmpty(clientIp))
            {
                clientIp = _httpContextAccessor.HttpContext?.Request.Headers["Cf-Connecting-Ip"];
            }

            return clientIp;
        }
    }
}
