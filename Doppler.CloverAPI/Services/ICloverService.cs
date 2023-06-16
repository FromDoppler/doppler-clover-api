using System.Threading.Tasks;
using Doppler.CloverAPI.Entities;

namespace Doppler.CloverAPI.Services
{
    public interface ICloverService
    {
        Task<bool> IsValidCreditCard(CreditCard creditCard, int clientId);

        Task<string> CreatePaymentAsync(string type, decimal chargeTotal, CreditCard creditCard, string clientId, string email);
    }
}
