using System.Threading.Tasks;
using Doppler.CloverAPI.Entities;

namespace Doppler.CloverAPI.Services
{
    public interface ICloverService
    {
        Task<bool> IsValidCreditCard(CreditCard creditCard, int clientId);

        Task<string> CreatePayment(string type, decimal chargeTotal, CreditCard creditCard, int clientId);
    }
}
