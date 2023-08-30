// unset:error

using System.Net.Http.Headers;

namespace Doppler.CloverAPI.Extensions
{
    public static class HttpRequestHeadersExtension
    {
        private const string DEFAULT_IP = "127.0.0.1";

        public static HttpRequestHeaders AddIpClientHeader(this HttpRequestHeaders httpRequestHeaders, string clientIp)
        {
            if (!string.IsNullOrEmpty(clientIp))
            {
                httpRequestHeaders.Add("x-forwarded-for", clientIp);
            }
            else
            {
                httpRequestHeaders.Add("x-forwarded-for", DEFAULT_IP);
            }

            return httpRequestHeaders;
        }
    }
}
