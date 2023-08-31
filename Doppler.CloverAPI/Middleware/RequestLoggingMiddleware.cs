using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Doppler.CloverAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger _logger;


        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var reqHeaders = context.Request.Headers;
            var headersStr = JsonSerializer.Serialize(reqHeaders.Select(x => $"{x.Key} = {x.Value}"));

            _logger.LogInformation($"Received headers: {headersStr}");

            await _next.Invoke(context);
        }
    }
}
