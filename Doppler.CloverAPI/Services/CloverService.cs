using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Doppler.CloverAPI.Entities;
using Doppler.CloverAPI.Entities.Clover;
using Doppler.CloverAPI.Exceptions;
using Doppler.CloverAPI.Extensions;
using Doppler.CloverAPI.Requests;
using Doppler.CloverAPI.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Doppler.CloverAPI.Services
{
    public class CloverService : ICloverService
    {
        private const string Ecomind = "ecom";
        private const string Currency = "usd";
        private const string ExternalReferenceId = "DopplerEmail";
        private const string RefundReason = "requested_by_customer";

        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public CloverService(IConfiguration configuration, ILogger<CloverService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> IsValidCreditCard(Entities.CreditCard creditCard, string clientId, string email, string clientIp)
        {
            await CreateChargeInClover(0, creditCard, clientId, email, true, clientIp);
            return true;
        }

        public async Task<string> CreatePaymentAsync(string type, decimal chargeTotal, Entities.CreditCard creditCard, string clientId, string email, string clientIp)
        {
            var isCreditCardValid = await IsValidCreditCard(creditCard, clientId, email, clientIp);
            return isCreditCardValid ? await CreateChargeInClover(chargeTotal, creditCard, clientId, email, false, clientIp) : string.Empty;
        }

        public async Task<string> CreateRefundAsync(decimal chargeTotal, string authorizationNumber, string email, CreditCard creditCard, string clientIp)
        {
            var response = string.Empty;

            var customer = await GetCustomerAsync(email, clientIp);
            if (customer == null)
            {
                var cardToken = await CreateCardTokenAsync(creditCard);
                customer = await CreateCustomerAsync(email, creditCard.CardHolderName, cardToken, clientIp);
            }

            var charge = await GetChargeByCustomerIdAndAuthorizationNumberAsync(customer.Id, authorizationNumber, clientIp);

            if (charge != null)
            {
                response = await CreateRefund(charge.Id, chargeTotal, clientIp);
            }

            return response;
        }

        public async Task<Customer> CreateCustomerAsync(string email, string name, Entities.CreditCard creditCard, string clientIp)
        {
            var cardToken = await CreateCardTokenAsync(creditCard);
            return await CreateCustomerAsync(email, creditCard.CardHolderName, cardToken, clientIp);
        }

        public async Task<Customer> UpdateCustomerAsync(string email, string name, Entities.CreditCard creditCard, string cloverCustomerId, string clientIp)
        {
            var cardToken = await CreateCardTokenAsync(creditCard);
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            client.DefaultRequestHeaders.AddIpClientHeader(clientIp);
            var updateCustomerUrl = string.Format(CultureInfo.CurrentCulture, _configuration["CloverSettings:UpdateCustomerUrl"], cloverCustomerId);

            var response = await client.PutAsJsonAsync(updateCustomerUrl,
                new CreateCustomerRequest
                {
                    Ecomind = Ecomind,
                    Email = email,
                    Name = name,
                    Source = cardToken
                });

            var xForwardedForValue = GetXForwardedForHeader(client.DefaultRequestHeaders);
            _logger.LogInformation($"x-forwarded-for: {xForwardedForValue}");

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

        public async Task<Customer> GetCustomerAsync(string email, string clientIp)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            client.DefaultRequestHeaders.AddIpClientHeader(clientIp);
            var getCustomerUrl = string.Format(CultureInfo.CurrentCulture, _configuration["CloverSettings:GetCustomerUrl"], _configuration["CloverSettings:MerchantId"], email);
            var response = await client.GetFromJsonAsync<GetCustomerResponse>(getCustomerUrl);

            var xForwardedForValue = GetXForwardedForHeader(client.DefaultRequestHeaders);
            _logger.LogInformation($"x-forwarded-for: {xForwardedForValue}");

            return response.Elements.FirstOrDefault();
        }

        public async Task RevokeCard(string customerId, string cardId, string clientIp)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            client.DefaultRequestHeaders.AddIpClientHeader(clientIp);
            var revokeCardUrl = string.Format(CultureInfo.CurrentCulture, _configuration["CloverSettings:RevokeCardUrl"], customerId, cardId);

            var response = await client.DeleteAsync(revokeCardUrl);
            var xForwardedForValue = GetXForwardedForHeader(client.DefaultRequestHeaders);
            _logger.LogInformation($"x-forwarded-for: {xForwardedForValue}");

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

        private async Task<Customer> CreateCustomerAsync(string email, string name, string source, string clientIp)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            client.DefaultRequestHeaders.AddIpClientHeader(clientIp);
            var createCustomerUrl = _configuration["CloverSettings:CreateCustomerUrl"];

            var response = await client.PostAsJsonAsync(createCustomerUrl,
                new CreateCustomerRequest
                {
                    Ecomind = Ecomind,
                    Email = email,
                    Name = name,
                    Source = source
                });

            var xForwardedForValue = GetXForwardedForHeader(client.DefaultRequestHeaders);
            _logger.LogInformation($"x-forwarded-for: {xForwardedForValue}");

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

        private async Task<string> CreateChargeInClover(decimal chargeTotal, Entities.CreditCard creditCard, string clientId, string email, bool isPreAuthorization, string clientIp)
        {
            try
            {
                string source;

                if (!isPreAuthorization)
                {
                    var customer = await GetCustomerAsync(email, clientIp);

                    if (customer == null)
                    {
                        var cardToken = await CreateCardTokenAsync(creditCard);
                        var customerCreated = await CreateCustomerAsync(email, creditCard.CardHolderName, cardToken, clientIp);

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
                client.DefaultRequestHeaders.AddIpClientHeader(clientIp);
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

                var xForwardedForValue = GetXForwardedForHeader(client.DefaultRequestHeaders);
                _logger.LogInformation($"x-forwarded-for: {xForwardedForValue}");

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

        private async Task<Charge> GetChargeByCustomerIdAndAuthorizationNumberAsync(string customerId, string authorizationNumber, string clientIp)
        {
            Charge charge = null;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
            client.DefaultRequestHeaders.AddIpClientHeader(clientIp);
            var getChargesByCustomerUrl = string.Format(CultureInfo.CurrentCulture, _configuration["CloverSettings:GetChargesByCustomerUrl"], customerId);
            var response = await client.GetFromJsonAsync<GetChargesResponse>(getChargesByCustomerUrl);

            var xForwardedForValue = GetXForwardedForHeader(client.DefaultRequestHeaders);
            _logger.LogInformation($"x-forwarded-for: {xForwardedForValue}");

            if (response != null && response.Data.Count > 0)
            {
                charge = response.Data.FirstOrDefault(c => c.AuthorizationNumber == authorizationNumber);
            }

            return charge;
        }

        private async Task<string> CreateRefund(string chargeId, decimal chargeTotal, string clientIp)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {_configuration["CloverSettings:EcommerceApiToken"]}");
                client.DefaultRequestHeaders.AddIpClientHeader(clientIp);
                var createRefundUrl = _configuration["CloverSettings:CreateRefundUrl"];

                var response = await client.PostAsJsonAsync(createRefundUrl,
                    new CreateRefundRequest
                    {
                        Amount = (int)(chargeTotal * 100),
                        Charge = chargeId,
                        ExternalReferenceId = ExternalReferenceId,
                        Reason = RefundReason
                    });

                var xForwardedForValue = GetXForwardedForHeader(client.DefaultRequestHeaders);
                _logger.LogInformation($"x-forwarded-for: {xForwardedForValue}");

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

        private static string GetXForwardedForHeader(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (var header in headers)
            {
                if (header.Key == "x-forwarded-for")
                {
                    return header.Value.FirstOrDefault();
                }
            }

            return "";
        }
    }
}
