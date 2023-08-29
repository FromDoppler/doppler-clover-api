using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Doppler.CloverAPI.DopplerSecurity;
using Doppler.CloverAPI.Exceptions;
using Doppler.CloverAPI.Requests;
using Doppler.CloverAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Doppler.CloverAPI.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ICloverService _cloverService;
        private readonly string _paymentType = "";

        public PaymentController(ICloverService cloverService)
        {
            _cloverService = cloverService;
        }

        [Authorize(Policies.OwnResourceOrSuperuser)]
        [HttpPost("/accounts/{accountname}/payment")]
        public async Task<IActionResult> CreatePayment([FromRoute] string accountname, [FromBody] PaymentRequest paymentRequest)
        {
            try
            {
                var paymentResponse = await _cloverService.CreatePaymentAsync(_paymentType, paymentRequest.ChargeTotal, paymentRequest.CreditCard, paymentRequest.ClientId, accountname);
                return Ok(paymentResponse);
            }
            catch (CloverApiException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ApiError);
            }
        }

        [Authorize(Policies.OwnResourceOrSuperuser)]
        [HttpPost("/accounts/{accountname}/refund")]
        public async Task<IActionResult> CreateRefund([FromRoute] string accountname, [FromBody] RefundRequest refundRequest)
        {

            try
            {
                var refundResponse = await _cloverService.CreateRefundAsync(refundRequest.ChargeTotal, refundRequest.ChargeAuthorizationNumber, accountname, refundRequest.CreditCard);
                return Ok(refundResponse);
            }
            catch (CloverApiException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ApiError);
            }
        }

        [Authorize(Policies.OwnResourceOrSuperuser)]
        [HttpPost("/accounts/{accountname}/creditcard/validate")]
        public async Task<IActionResult> ValidateCreditCard([FromRoute] string accountname, [FromBody] CreditCardRequest creditCardRequest)
        {
            try
            {
                var isValidResponse = await _cloverService.IsValidCreditCard(creditCardRequest.CreditCard, creditCardRequest.ClientId, accountname);
                return Ok(isValidResponse);
            }
            catch (CloverApiException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ApiError);
            }
        }

        [HttpGet("/clientip")]
        public IActionResult TestIpClient()
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress.ToString();

            return Ok(new { clientIp });
        }

        [HttpGet("/clientipV2")]
        public IActionResult TestIpClientV3()
        {
            var xRealIpExists = HttpContext.Request.Headers.TryGetValue("X-Real-IP", out var xRealIp);
            if (xRealIpExists)
            {
                if (IPAddress.TryParse(xRealIp, out var address))
                {
                    var isValidIP = (address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6);

                    if (isValidIP)
                    {
                        return Ok(new { clientIp = address.ToString() });
                    }
                }
            }

            IPAddress remoteIpAddress = null;
            var headerValues = HttpContext.Request.Headers["X-Forwarded-For"];
            var forwardedFor = headerValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                foreach (var ip in ips)
                {
                    if (IPAddress.TryParse(ip, out var address) &&
                        (address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
                    {
                        remoteIpAddress = address;
                        break;
                    }
                }
            }

            return remoteIpAddress != null
                ? Ok(new { clientIp = remoteIpAddress.ToString() })
                : Ok(new { clientIp = HttpContext.Connection.RemoteIpAddress.ToString() });
        }

        [HttpGet("/clientipV3")]
        public IActionResult TestIpClientV4()
        {
            var exists = HttpContext.Request.Headers.TryGetValue("CF-Connecting-IP", out var xRealIp);
            if (exists)
            {
                var ip = xRealIp.ToString();
                return Ok(new { clientIp = ip });
            }
            return Ok();
        }

        [HttpGet("/clientipV4")]
        public IActionResult TestIpClientV5()
        {
            var exists = HttpContext.Request.Headers.TryGetValue("HTTP_CF_CONNECTING_IP", out var xRealIp);
            if (exists)
            {
                var ip = xRealIp.ToString();
                return Ok(new { clientIp = ip });
            }
            return Ok();
        }

        [HttpGet("/clientipV5")]
        public IActionResult TestIpClientV6()
        {
            var exists = HttpContext.Request.Headers.TryGetValue("REMOTE_ADDR", out var xRealIp);
            if (exists)
            {
                var ip = xRealIp.ToString();
                return Ok(new { clientIp = ip });
            }
            return Ok();
        }

        [HttpGet("/clientipV6")]
        public IActionResult TestIpClientV7()
        {     
            var ip = HttpContext.GetServerVariable("REMOTE_ADDR");
            return Ok(new { clientIp = ip });
        }
    }
}
