using System.Threading.Tasks;
using Doppler.CloverAPI.Entities;
using Doppler.CloverAPI.Entities.Clover;

namespace Doppler.CloverAPI.Services
{
    public interface ICloverService
    {
        Task<bool> IsValidCreditCard(Entities.CreditCard creditCard, string clientId, string email, string clientIp);

        Task<string> CreatePaymentAsync(string type, decimal chargeTotal, Entities.CreditCard creditCard, string clientId, string email, string clientIp);

        Task<string> CreateRefundAsync(decimal chargeTotal, string authorizationNumber, string email, CreditCard creditCard, string clientIp);

        Task<Customer> CreateCustomerAsync(string email, string name, Entities.CreditCard creditCard, string clientIp);

        Task<Customer> UpdateCustomerAsync(string email, string name, Entities.CreditCard creditCard, string cloverCustomerId, string clientIp);

        Task<Customer> GetCustomerAsync(string email, string clientIp);

        Task RevokeCard(string customerId, string cardId, string clientIp);
    }
}
