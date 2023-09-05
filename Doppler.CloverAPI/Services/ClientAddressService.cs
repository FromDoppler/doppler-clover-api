using System.Threading.Tasks;
using Doppler.CloverAPI.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Doppler.CloverAPI.Services
{
    public class ClientAddressService : IClientAddressService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoginXUserRepository _loginXUserRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientAddressService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, ILoginXUserRepository loginXUserRepository)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _loginXUserRepository = loginXUserRepository;
        }

        public async Task<string> GetIpAddress(string email)
        {
            var userId = await _userRepository.GetUserIdByEmail(email);
            var clientIp = await _loginXUserRepository.GetIpAddressOfLastLoginByUserId(userId);

            if (string.IsNullOrEmpty(clientIp))
            {
                clientIp = await _userRepository.GetIpAddressByEmail(email);
            }

            if (string.IsNullOrEmpty(clientIp))
            {
                clientIp = _httpContextAccessor.HttpContext?.Request.Headers["Cf-Connecting-Ip"];
            }

            return clientIp;
        }
    }
}
