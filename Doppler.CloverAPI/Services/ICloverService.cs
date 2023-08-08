using System.Threading.Tasks;
using Doppler.CloverAPI.Entities;
using Doppler.CloverAPI.Entities.Clover;

namespace Doppler.CloverAPI.Services
{
    public interface ICloverService
    {
        Task<bool> IsValidCreditCard(Entities.CreditCard creditCard, string clientId, string email);

        Task<string> CreatePaymentAsync(string type, decimal chargeTotal, Entities.CreditCard creditCard, string clientId, string email);

        Task<string> CreateRefundAsync(decimal chargeTotal, string authorizationNumber, string email, CreditCard creditCard);

        Task<Customer> CreateCustomerAsync(string email, string name, Entities.CreditCard creditCard);

        Task<Customer> UpdateCustomerAsync(string email, string name, Entities.CreditCard creditCard, string cloverCustomerId);

        Task<Customer> GetCustomerAsync(string email);

        Task RevokeCard(string customerId, string cardId);
    }
}
