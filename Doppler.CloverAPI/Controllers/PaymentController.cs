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
            var refundResponse = await _cloverService.CreateRefundAsync(refundRequest.ChargeTotal, refundRequest.ChargeAuthorizationNumber, accountname);
            return Ok(refundResponse);
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
    }
}
