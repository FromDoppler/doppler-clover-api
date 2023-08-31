using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Doppler.CloverAPI.Entities;
using Doppler.CloverAPI.Entities.Clover;
using Doppler.CloverAPI.Exceptions;
using Doppler.CloverAPI.Requests;
using Doppler.CloverAPI.Response;
using Microsoft.Extensions.Configuration;

namespace Doppler.CloverAPI.Services
{
    public class CloverService : ICloverService
    {
        private const string Ecomind = "ecom";
        private const string Currency = "usd";
        private const string ExternalReferenceId = "DopplerEmail";
        private const string RefundReason = "requested_by_customer";

        private readonly IConfiguration _configuration;

        public CloverService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> IsValidCreditCard(Entities.CreditCard creditCard, string clientId, string email)
        {
            await CreateChargeInClover(0, creditCard, clientId, email, true);
            return true;
        }

        public async Task<string> CreatePaymentAsync(string type, decimal chargeTotal, Entities.CreditCard creditCard, string clientId, string email)
        {
            var isCreditCardValid = await IsValidCreditCard(creditCard, clientId, email);
            return isCreditCardValid ? await CreateChargeInClover(chargeTotal, creditCard, clientId, email, false) : string.Empty;
        }

        public async Task<string> CreateRefundAsync(decimal chargeTotal, string authorizationNumber, string email, CreditCard creditCard)
        {
            var response = string.Empty;

            var customer = await GetCustomerAsync(email);
            if (customer == null)
            {
                var cardToken = await CreateCardTokenAsync(creditCard);
                customer = await CreateCustomerAsync(email, creditCard.CardHolderName, cardToken);
            }

            var charge = await GetChargeByCustomerIdAndAuthorizationNumberAsync(customer.Id, authorizationNumber);

            if (charge != null)
            {
                response = await CreateRefund(charge.Id, chargeTotal);
            }

            return response;
        }

        public async Task<Customer> CreateCustomerAsync(string email, string name, Entities.CreditCard creditCard)
        {
            var cardToken = await CreateCardTokenAsync(creditCard);
            return await CreateCustomerAsync(email, creditCard.CardHolderName, cardToken);
        }

        public async Task<Customer> UpdateCustomerAsync(string email, string name, Entities.CreditCard creditCard, string cloverCustomerId)
        {
            var cardToken = await CreateCardTokenAsync(creditCard);
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            var updateCustomerUrl = string.Format(CultureInfo.CurrentCulture, _configuration["CloverSettings:UpdateCustomerUrl"], cloverCustomerId);

            var response = await client.PutAsJsonAsync(updateCustomerUrl,
                new CreateCustomerRequest
                {
                    Ecomind = Ecomind,
                    Email = email,
                    Name = name,
                    Source = cardToken
                });

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CreateCustomerResponse>();
                return new Customer { Id = customer.Id };
            }
            else
            {
                var result = await response.Content.ReadFromJsonAsync<ApiError>();
                var exception = new CloverApiException(result.Error.Code, result.Error.Message) { ApiError = result };

                throw exception;
            }
        }

        public async Task<Customer> GetCustomerAsync(string email)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            var getCustomerUrl = string.Format(CultureInfo.CurrentCulture, _configuration["CloverSettings:GetCustomerUrl"], _configuration["CloverSettings:MerchantId"], email);
            var response = await client.GetFromJsonAsync<GetCustomerResponse>(getCustomerUrl);

            return response.Elements.FirstOrDefault();
        }

        public async Task RevokeCard(string customerId, string cardId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            var revokeCardUrl = string.Format(CultureInfo.CurrentCulture, _configuration["CloverSettings:RevokeCardUrl"], customerId, cardId);

            var response = await client.DeleteAsync(revokeCardUrl);

            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiError>();
                var exception = new CloverApiException(result.Error.Code, result.Error.Message) { ApiError = result };

                throw exception;
            }
        }

        private async Task<string> CreateCardTokenAsync(Entities.CreditCard creditCard)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", $"{_configuration["CloverSettings:ApiAccessKey"]}");
            var createTokenUrl = _configuration["CloverSettings:CreateCardTokenUrl"];

            var response = await client.PostAsJsonAsync(createTokenUrl,
                new CardTokenRequest
                {
                    Card = new Entities.Clover.Card
                    {
                        Brand = creditCard.CardType,
                        Cvv = creditCard.SecurityCode,
                        ExpMonth = creditCard.CardExpMonth,
                        ExpYear = creditCard.CardExpYear,
                        First6 = creditCard.CardNumber[0..6],
                        Last4 = creditCard.CardNumber[^4..],
                        Name = creditCard.CardHolderName.Split(' ').Length > 1 ? creditCard.CardHolderName : $"{creditCard.CardHolderName} {creditCard.CardHolderName}",
                        Number = creditCard.CardNumber
                    }
                });

            if (response.IsSuccessStatusCode)
            {
                var cardToken = await response.Content.ReadFromJsonAsync<CardTokenResponse>();
                return cardToken.Id;
            }
            else
            {
                var result = await response.Content.ReadFromJsonAsync<ApiError>();
                var exception = new CloverApiException(result.Error.Code, result.Error.Message) { ApiError = result };

                throw exception;
            }
        }

        private async Task<Customer> CreateCustomerAsync(string email, string name, string source)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            var createCustomerUrl = _configuration["CloverSettings:CreateCustomerUrl"];

            var response = await client.PostAsJsonAsync(createCustomerUrl,
                new CreateCustomerRequest
                {
                    Ecomind = Ecomind,
                    Email = email,
                    Name = name,
                    Source = source
                });

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CreateCustomerResponse>();
                return new Customer { Id = customer.Id };
            }
            else
            {
                var result = await response.Content.ReadFromJsonAsync<ApiError>();
                var exception = new CloverApiException(result.Error.Code, result.Error.Message) { ApiError = result };

                throw exception;
            }
        }

        private async Task<string> CreateChargeInClover(decimal chargeTotal, Entities.CreditCard creditCard, string clientId, string email, bool isPreAuthorization)
        {
            try
            {
                string source;

                if (!isPreAuthorization)
                {
                    var customer = await GetCustomerAsync(email);

                    if (customer == null)
                    {
                        var cardToken = await CreateCardTokenAsync(creditCard);
                        var customerCreated = await CreateCustomerAsync(email, creditCard.CardHolderName, cardToken);

                        source = customerCreated != null ? customerCreated.Id : string.Empty;
                    }
                    else
                    {
                        source = customer.Id;
                    }
                }
                else
                {
                    var cardToken = await CreateCardTokenAsync(creditCard);
                    source = cardToken;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
                var createPaymentUrl = _configuration["CloverSettings:CreatePaymentUrl"];

                var response = await client.PostAsJsonAsync(createPaymentUrl,
                    new CreateChargeRequest
                    {
                        Amount = (int)(chargeTotal * 100),
                        Capture = !isPreAuthorization,
                        Currency = Currency,
                        Description = clientId,
                        Ecomind = Ecomind,
                        ExternalReferenceId = ExternalReferenceId,
                        Source = source
                    });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CreateChargeResponse>();
                    return result.AuthorizationNumber;
                }
                else
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiError>();
                    var exception = new CloverApiException(result.Error.Code, result.Error.Message) { ApiError = result };

                    throw exception;
                }
            }
            catch (CloverApiException ex)
            {
                throw ex;
            }
        }

        private async Task<Charge> GetChargeByCustomerIdAndAuthorizationNumberAsync(string customerId, string authorizationNumber)
        {
            Charge charge = null;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            var getChargesByCustomerUrl = string.Format(CultureInfo.CurrentCulture, _configuration["CloverSettings:GetChargesByCustomerUrl"], customerId);
            var response = await client.GetFromJsonAsync<GetChargesResponse>(getChargesByCustomerUrl);

            if (response != null && response.Data.Count > 0)
            {
                charge = response.Data.FirstOrDefault(c => c.AuthorizationNumber == authorizationNumber);
            }

            return charge;
        }

        private async Task<string> CreateRefund(string chargeId, decimal chargeTotal)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
                var createRefundUrl = _configuration["CloverSettings:CreateRefundUrl"];

                var response = await client.PostAsJsonAsync(createRefundUrl,
                    new CreateRefundRequest
                    {
                        Amount = (int)(chargeTotal * 100),
                        Charge = chargeId,
                        ExternalReferenceId = ExternalReferenceId,
                        Reason = RefundReason
                    });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CreateRefundResponse>();
                    return result.Metadata.AuthCode;
                }
                else
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiError>();
                    var exception = new CloverApiException(result.Error.Code, result.Error.Message) { ApiError = result };

                    throw exception;
                }
            }
            catch (CloverApiException ex)
            {
                throw ex;
            }
        }
    }
}
