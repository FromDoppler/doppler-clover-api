using Dapper;
using Doppler.CloverAPI.Infrastructure;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.CloverAPI.Services
{
    public class ClientAddressServiceTest
    {
        [Fact]
        public async Task GetIpAddress_Should_Return_Db_ClientIp_When_Exists_In_Db()
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContextMock = new Mock<HttpContext>();
            var expectedClientIp = "111.111.111.112";

            userRepositoryMock.Setup(x => x.GetIpAddressByEmail(It.IsAny<string>())).ReturnsAsync(expectedClientIp);

            httpContextMock.Setup(x => x.Request.Headers["Cf-Connecting-Ip"]).Returns("127.0.0.1");

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var clientAddressService = new ClientAddressService(userRepositoryMock.Object, httpContextAccessorMock.Object);

            var result = await clientAddressService.GetIpAddress("test@makingsense.com");

            Assert.Equal(expectedClientIp, result);
        }

        [Fact]
        public async Task GetIpAddress_Should_Return_Header_ClientIp_When_DoesNot_Exists_In_Db()
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContextMock = new Mock<HttpContext>();
            var expectedClientIp = "111.111.111.111";

            userRepositoryMock.Setup(x => x.GetIpAddressByEmail(It.IsAny<string>())).ReturnsAsync("");

            httpContextMock.Setup(x => x.Request.Headers["Cf-Connecting-Ip"]).Returns(expectedClientIp);

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var clientAddressService = new ClientAddressService(userRepositoryMock.Object, httpContextAccessorMock.Object);

            var result = await clientAddressService.GetIpAddress("test@makingsense.com");

            Assert.Equal(expectedClientIp, result);
        }
    }
}
