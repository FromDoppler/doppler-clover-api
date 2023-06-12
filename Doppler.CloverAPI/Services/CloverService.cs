using System.Threading.Tasks;
using Doppler.CloverAPI.Entities;

namespace Doppler.CloverAPI.Services
{
    public class CloverService : ICloverService
    {
        public Task<bool> IsValidCreditCard(CreditCard creditCard, int clientId)
        {
            //TODO: Remove it and use the clover api
            return Task.FromResult(true);
        }

        public Task<string> CreatePayment(string type, decimal chargeTotal, CreditCard creditCard, int clientId)
        {
            //TODO: Remove it and use the clover api
            return Task.FromResult("AUTH123");
        }
    }
}
