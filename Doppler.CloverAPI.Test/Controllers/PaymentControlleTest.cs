using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Doppler.CloverAPI.Entities;
using Doppler.CloverAPI.Requests;
using Doppler.CloverAPI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Doppler.CloverAPI.Controllers
{
    public class PaymentControlleTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private const string TOKEN_ACCOUNT_123_TEST1_AT_EXAMPLE_DOT_COM_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjEyMywidW5pcXVlX25hbWUiOiJ0ZXN0MUBleGFtcGxlLmNvbSIsInJvbGUiOiJVU0VSIiwiZXhwIjoyMDAwMDAwMDAwfQ.C4shc2SZqolHSpxSLU3GykR0A0Zyh0fofqNirS3CmeY4ZerofgRry7m9AMFyn1SG-rmLDpFJIObFA2dn7nN6uKf5gCTEIwGAB71LfAeVaEfOeF1SvLJh3-qGXknqinsrX8tuBhoaHmpWpvdp0PW-8PmLuBq-D4GWBGyrP73sx_qQi322E2_PJGfudygbahdQ9v4SnBh7AOlaLKSXhGRT-qsMCxZJXpHM7cZsaBkOlo8x_LEWbbkf7Ub6q3mWaQsR30NlJVTaRMY9xWrRMV_iZocREg2EI33mMBa5zhuyQ-hXENp5M9FgS_9B-j3LpFJoJyVFZG2beBRxU8tnqKan3A";

        public PaymentControlleTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreatePayment_Should_Return_AuthorizationNumber_When_Payment_In_Clover_Is_Successfully()
        {
            // Arrange
            var authorizationNumber = "T123456";
            var paymentRequest = new PaymentRequest()
            {
                ChargeTotal = 100,
                ClientId = "1",
                CreditCard = new Entities.CreditCard()
            };

            var cloverServiceMock = new Mock<ICloverService>();
            cloverServiceMock
                .Setup(x => x.CreatePaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CreditCard>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(authorizationNumber);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(cloverServiceMock.Object);
                });

            }).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "/accounts/test1@example.com/payment")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_ACCOUNT_123_TEST1_AT_EXAMPLE_DOT_COM_EXPIRE_20330518}" } },
                Content = JsonContent.Create(paymentRequest)
            };

            // Act
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(authorizationNumber, result);
        }

        [Fact]
        public async Task CreateRefund_Should_Return_AuthorizationNumber_When_Payment_In_Clover_Is_Successfully()
        {
            // Arrange
            var authorizationNumber = "TR123456";
            var paymentRequest = new PaymentRequest()
            {
                ChargeTotal = 100,
                ClientId = "1",
                CreditCard = new Entities.CreditCard()
            };

            var cloverServiceMock = new Mock<ICloverService>();
            cloverServiceMock
                .Setup(x => x.CreateRefundAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(authorizationNumber);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(cloverServiceMock.Object);
                });

            }).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "/accounts/test1@example.com/refund")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_ACCOUNT_123_TEST1_AT_EXAMPLE_DOT_COM_EXPIRE_20330518}" } },
                Content = JsonContent.Create(paymentRequest)
            };

            // Act
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(authorizationNumber, result);
        }

        [Fact]
        public async Task ValidateCreditCard_Should_Return_True_When_CreditCard_Is_Valid()
        {
            // Arrange
            var creditCardRequest = new CreditCardRequest()
            {
                ClientId = "1",
                CreditCard = new Entities.CreditCard()
            };

            var cloverServiceMock = new Mock<ICloverService>();
            cloverServiceMock
                .Setup(x => x.IsValidCreditCard(It.IsAny<CreditCard>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(cloverServiceMock.Object);
                });

            }).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "/accounts/test1@example.com/creditcard/validate")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_ACCOUNT_123_TEST1_AT_EXAMPLE_DOT_COM_EXPIRE_20330518}" } },
                Content = JsonContent.Create(creditCardRequest)
            };

            // Act
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", result);
        }

        [Fact]
        public async Task ValidateCreditCard_Should_Return_False_When_CreditCard_Is_Invalid()
        {
            // Arrange
            var creditCardRequest = new CreditCardRequest()
            {
                ClientId = "1",
                CreditCard = new Entities.CreditCard()
            };

            var cloverServiceMock = new Mock<ICloverService>();
            cloverServiceMock
                .Setup(x => x.IsValidCreditCard(It.IsAny<CreditCard>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(cloverServiceMock.Object);
                });

            }).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "/accounts/test1@example.com/creditcard/validate")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_ACCOUNT_123_TEST1_AT_EXAMPLE_DOT_COM_EXPIRE_20330518}" } },
                Content = JsonContent.Create(creditCardRequest)
            };

            // Act
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("false", result);
        }
    }
}
