using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Doppler.CloverAPI.Entities;
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

        private readonly IConfiguration _configuration;

        public CloverService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> IsValidCreditCard(CreditCard creditCard, string clientId, string email)
        {
            await CreateChargeInClover(0, creditCard, clientId, email, true);
            return true;
        }

        public async Task<string> CreatePaymentAsync(string type, decimal chargeTotal, CreditCard creditCard, string clientId, string email)
        {
            return await CreateChargeInClover(chargeTotal, creditCard, clientId, email, false);
        }

        private async Task<string> CreateCardTokenAsync(CreditCard creditCard)
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
                        Name = creditCard.CardHolderName,
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

        private async Task<string> GetCustomerAsync(string email)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            var createTokenUrl = string.Format(CultureInfo.CurrentCulture, _configuration["CloverSettings:GetPaymentUrl"], _configuration["CloverSettings:MerchantId"], email);
            var response = await client.GetFromJsonAsync<GetCustomerResponse>(createTokenUrl);

            return response.Elements.Count > 0 ? response.Elements.FirstOrDefault().Id : string.Empty;
        }

        private async Task<string> CreateCustomerAsync(string email, string name, string source)
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
                return customer.Id;
            }
            else
            {
                var result = await response.Content.ReadFromJsonAsync<ApiError>();
                var exception = new CloverApiException(result.Error.Code, result.Error.Message) { ApiError = result };

                throw exception;
            }
        }

        private async Task<string> CreateChargeInClover(decimal chargeTotal, CreditCard creditCard, string clientId, string email, bool isPreAuthorization)
        {
            string source;

            if (!isPreAuthorization)
            {
                source = await GetCustomerAsync(email);

                if (string.IsNullOrEmpty(source))
                {
                    var cardToken = await CreateCardTokenAsync(creditCard);
                    source = await CreateCustomerAsync(email, creditCard.CardHolderName, cardToken);
                }
            }
            else
            {
                var cardToken = await CreateCardTokenAsync(creditCard);
                source = await CreateCustomerAsync(email, creditCard.CardHolderName, cardToken);
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            var createPaymentUrl = _configuration["CloverSettings:CreatePaymentUrl"];

            var response = await client.PostAsJsonAsync(createPaymentUrl,
                new CreateChargeRequest
                {
                    Amount = (int)chargeTotal * 100,
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
    }
}
