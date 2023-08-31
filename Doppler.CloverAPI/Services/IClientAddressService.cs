using System.Threading.Tasks;

namespace Doppler.CloverAPI.Services
{
    public interface IClientAddressService
    {
        Task<string> GetIpAddress(string email);
    }
}
