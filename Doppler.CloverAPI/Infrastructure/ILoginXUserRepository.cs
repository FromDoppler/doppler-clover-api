using System.Threading.Tasks;

namespace Doppler.CloverAPI.Infrastructure
{
    public interface ILoginXUserRepository
    {
        Task<string> GetIpAddressOfLastLoginByUserId(int userId);
    }
}
