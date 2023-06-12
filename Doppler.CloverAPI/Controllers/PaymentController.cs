using System.Threading.Tasks;
using Doppler.CloverAPI.Requests;
using Doppler.CloverAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Doppler.CloverAPI.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ICloverService _cloverService;
        private readonly string _paymentType = "";
        private readonly string _refundType = "";

        public PaymentController(ICloverService cloverService)
        {
            _cloverService = cloverService;
        }

        [HttpPost("/accounts/{accountname}/payment")]
        public async Task<IActionResult> CreatePayment([FromRoute] string accountname, [FromBody] PaymentRequest paymentRequest)
        {
            var paymentResponse = await _cloverService.CreatePayment(_paymentType, paymentRequest.ChargeTotal, paymentRequest.CreditCard, paymentRequest.ClientId);
            return Ok(paymentResponse);
        }

        [HttpPost("/accounts/{accountname}/refund")]
        public async Task<IActionResult> CreateRefund([FromRoute] string accountname, [FromBody] PaymentRequest paymentRequest)
        {
            var paymentResponse = await _cloverService.CreatePayment(_refundType, paymentRequest.ChargeTotal, paymentRequest.CreditCard, paymentRequest.ClientId);
            return Ok(paymentResponse);
        }

        [HttpPost("/accounts/{accountname}/creditcard/validate")]
        public async Task<IActionResult> ValidateCreditCard([FromRoute] string accountname, [FromBody] CreditCardRequest creditCardRequest)
        {
            var isValidResponse = await _cloverService.IsValidCreditCard(creditCardRequest.CreditCard, creditCardRequest.ClientId);
            return Ok(isValidResponse);
        }
    }
}
