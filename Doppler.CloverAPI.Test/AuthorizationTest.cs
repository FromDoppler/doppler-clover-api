using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.CloverAPI;

public class AuthorizationTest
    : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;

    public AuthorizationTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData("/hello/anonymous", HttpStatusCode.OK)]
    public async Task GET_helloAnonymous_should_not_require_token(string url, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData("/hello/anonymous", TestUsersData.Token_Empty, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Expire2096_10_02, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Expire2001_09_08, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Broken, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Superuser_Expire2096_10_02, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Superuser_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Superuser_Expire2001_09_08, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_SuperuserFalse_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2096_10_02, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/hello/anonymous", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2001_09_08, HttpStatusCode.OK)]
    public async Task GET_helloAnonymous_should_accept_any_token(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { { "Authorization", $"Bearer {token}" } }
        };

        // Act
        var response = await client.SendAsync(request);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData("/hello/valid-token", HttpStatusCode.Unauthorized)]
    [InlineData("/hello/superuser", HttpStatusCode.Unauthorized)]
    [InlineData("/accounts/123/hello", HttpStatusCode.Unauthorized)]
    [InlineData("/accounts/test1@test.com/hello", HttpStatusCode.Unauthorized)]
    public async Task GET_authenticated_endpoints_should_require_token(string url, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData("/hello/valid-token", TestUsersData.Token_Empty, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/hello/valid-token", TestUsersData.Token_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/hello/valid-token", TestUsersData.Token_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/hello/valid-token", TestUsersData.Token_Broken, HttpStatusCode.Unauthorized, "invalid_token", "")]
    [InlineData("/hello/valid-token", TestUsersData.Token_Superuser_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/hello/valid-token", TestUsersData.Token_Superuser_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/hello/valid-token", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/hello/valid-token", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/hello/superuser", TestUsersData.Token_Empty, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/hello/superuser", TestUsersData.Token_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/hello/superuser", TestUsersData.Token_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/hello/superuser", TestUsersData.Token_Broken, HttpStatusCode.Unauthorized, "invalid_token", "")]
    [InlineData("/hello/superuser", TestUsersData.Token_Superuser_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/hello/superuser", TestUsersData.Token_Superuser_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/hello/superuser", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/hello/superuser", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Empty, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Broken, HttpStatusCode.Unauthorized, "invalid_token", "")]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Superuser_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Superuser_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Empty, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Broken, HttpStatusCode.Unauthorized, "invalid_token", "")]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Superuser_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Superuser_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2096_10_02, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2001_09_08, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
    public async Task GET_authenticated_endpoints_should_require_a_valid_token(string url, string token, HttpStatusCode expectedStatusCode, string error, string extraErrorInfo)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { { "Authorization", $"Bearer {token}" } }
        };

        // Act
        var response = await client.SendAsync(request);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.StartsWith("Bearer", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains($"error=\"{error}\"", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains(extraErrorInfo, response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData("/hello/valid-token", TestUsersData.Token_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/hello/valid-token", TestUsersData.Token_Superuser_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/hello/valid-token", TestUsersData.Token_SuperuserFalse_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/hello/valid-token", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2033_05_18, HttpStatusCode.OK)]
    public async Task GET_Token_should_accept_valid_token(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { { "Authorization", $"Bearer {token}" } }
        };

        // Act
        var response = await client.SendAsync(request);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData("/hello/superuser", TestUsersData.Token_Expire2033_05_18, HttpStatusCode.Forbidden)]
    [InlineData("/hello/superuser", TestUsersData.Token_SuperuserFalse_Expire2033_05_18, HttpStatusCode.Forbidden)]
    [InlineData("/hello/superuser", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2033_05_18, HttpStatusCode.Forbidden)]
    public async Task GET_helloSuperUser_should_require_a_valid_token_with_isSU_flag(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { { "Authorization", $"Bearer {token}" } }
        };

        // Act
        var response = await client.SendAsync(request);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData("/hello/superuser", TestUsersData.Token_Superuser_Expire2033_05_18, HttpStatusCode.OK)]
    public async Task GET_helloSuperUser_should_accept_valid_token_with_isSU_flag(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { { "Authorization", $"Bearer {token}" } }
        };

        // Act
        var response = await client.SendAsync(request);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Expire2033_05_18, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/123/hello", TestUsersData.Token_SuperuserFalse_Expire2033_05_18, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/456/hello", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2033_05_18, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Expire2033_05_18, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_SuperuserFalse_Expire2033_05_18, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/test2@test.com/hello", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2033_05_18, HttpStatusCode.Forbidden)]
    public async Task GET_account_endpoint_should_require_a_valid_token_with_isSU_flag_or_a_token_for_the_right_account(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { { "Authorization", $"Bearer {token}" } }
        };

        // Act
        var response = await client.SendAsync(request);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Superuser_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/accounts/123/hello", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Superuser_Expire2033_05_18, HttpStatusCode.OK)]
    [InlineData("/accounts/test1@test.com/hello", TestUsersData.Token_Account_123_test1AtTestDotCom_Expire2033_05_18, HttpStatusCode.OK)]
    public async Task GET_account_endpoint_should_accept_valid_token_with_isSU_flag_or_a_token_for_the_right_account(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { { "Authorization", $"Bearer {token}" } }
        };

        // Act
        var response = await client.SendAsync(request);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }
}
