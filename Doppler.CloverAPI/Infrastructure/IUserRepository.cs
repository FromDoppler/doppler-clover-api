using System.Threading.Tasks;

namespace Doppler.CloverAPI.Infrastructure
{
    public interface IUserRepository
    {
        Task<string> GetIpAddressByEmail(string email);

        Task<int> GetUserIdByEmail(string email);
    }
}
