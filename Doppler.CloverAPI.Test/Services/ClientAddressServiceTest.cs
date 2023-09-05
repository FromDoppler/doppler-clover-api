using System.Threading.Tasks;
using Doppler.CloverAPI.Infrastructure;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Doppler.CloverAPI.Services
{
    public class ClientAddressServiceTest
    {
        [Fact]
        public async Task GetIpAddress_Should_Return_LastLogin_ClientIp_When_Exists()
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            var loginXUserMock = new Mock<ILoginXUserRepository>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContextMock = new Mock<HttpContext>();
            var expectedClientIp = "111.111.111.112";
            var userId = 123;

            userRepositoryMock.Setup(x => x.GetUserIdByEmail(It.IsAny<string>())).ReturnsAsync(userId);

            loginXUserMock.Setup(x => x.GetIpAddressOfLastLoginByUserId(userId)).ReturnsAsync(expectedClientIp);

            httpContextMock.Setup(x => x.Request.Headers["Cf-Connecting-Ip"]).Returns("123.123.111.111");

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var clientAddressService = new ClientAddressService(userRepositoryMock.Object, httpContextAccessorMock.Object, loginXUserMock.Object);

            var result = await clientAddressService.GetIpAddress("test@makingsense.com");

            userRepositoryMock.Verify(x => x.GetUserIdByEmail(It.IsAny<string>()), Times.Once());
            loginXUserMock.Verify(x => x.GetIpAddressOfLastLoginByUserId(It.IsAny<int>()), Times.Once());

            userRepositoryMock.Verify(x => x.GetIpAddressByEmail(It.IsAny<string>()), Times.Never());
            Assert.Equal(expectedClientIp, result);
        }

        [Fact]
        public async Task GetIpAddress_Should_Return_Registration_ClientIp_When_Exists()
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            var loginXUserMock = new Mock<ILoginXUserRepository>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContextMock = new Mock<HttpContext>();
            var expectedClientIp = "111.111.111.112";
            var emptyIp = string.Empty;
            var userId = 123;

            userRepositoryMock.Setup(x => x.GetUserIdByEmail(It.IsAny<string>())).ReturnsAsync(userId);

            loginXUserMock.Setup(x => x.GetIpAddressOfLastLoginByUserId(userId)).ReturnsAsync(emptyIp);

            userRepositoryMock.Setup(x => x.GetIpAddressByEmail(It.IsAny<string>())).ReturnsAsync(expectedClientIp);

            httpContextMock.Setup(x => x.Request.Headers["Cf-Connecting-Ip"]).Returns("123.123.111.111");

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var clientAddressService = new ClientAddressService(userRepositoryMock.Object, httpContextAccessorMock.Object, loginXUserMock.Object);

            var result = await clientAddressService.GetIpAddress("test@makingsense.com");

            userRepositoryMock.Verify(x => x.GetUserIdByEmail(It.IsAny<string>()), Times.Once());
            loginXUserMock.Verify(x => x.GetIpAddressOfLastLoginByUserId(It.IsAny<int>()), Times.Once());
            userRepositoryMock.Verify(x => x.GetIpAddressByEmail(It.IsAny<string>()), Times.Once());
            Assert.Equal(expectedClientIp, result);
        }

        [Fact]
        public async Task GetIpAddress_Should_Return_Header_ClientIp_When_Is_Not_Stored_In_The_Database()
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            var loginXUserMock = new Mock<ILoginXUserRepository>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var httpContextMock = new Mock<HttpContext>();
            var expectedClientIp = "111.111.111.112";
            var emptyIp = string.Empty;
            var userId = 123;

            userRepositoryMock.Setup(x => x.GetUserIdByEmail(It.IsAny<string>())).ReturnsAsync(userId);

            loginXUserMock.Setup(x => x.GetIpAddressOfLastLoginByUserId(userId)).ReturnsAsync(emptyIp);

            userRepositoryMock.Setup(x => x.GetIpAddressByEmail(It.IsAny<string>())).ReturnsAsync(emptyIp);

            httpContextMock.Setup(x => x.Request.Headers["Cf-Connecting-Ip"]).Returns(expectedClientIp);

            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var clientAddressService = new ClientAddressService(userRepositoryMock.Object, httpContextAccessorMock.Object, loginXUserMock.Object);

            var result = await clientAddressService.GetIpAddress("test@makingsense.com");

            userRepositoryMock.Verify(x => x.GetUserIdByEmail(It.IsAny<string>()), Times.Once());
            loginXUserMock.Verify(x => x.GetIpAddressOfLastLoginByUserId(It.IsAny<int>()), Times.Once());
            userRepositoryMock.Verify(x => x.GetIpAddressByEmail(It.IsAny<string>()), Times.Once());
            Assert.Equal(expectedClientIp, result);
        }
    }
}
