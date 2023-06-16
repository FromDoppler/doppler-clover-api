using System.Linq;
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
    public class CustomerController : Controller
    {
        private readonly ICloverService _cloverService;

        public CustomerController(ICloverService cloverService)
        {
            _cloverService = cloverService;
        }

        [Authorize(Policies.OwnResourceOrSuperuser)]
        [HttpGet("/accounts/{accountname}/customer")]
        public async Task<IActionResult> GetCustomer([FromRoute] string accountname)
        {
            try
            {
                var customerResponse = await _cloverService.GetCustomerAsync(accountname);
                return Ok(customerResponse);
            }
            catch (CloverApiException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ApiError);
            }
        }

        [Authorize(Policies.OwnResourceOrSuperuser)]
        [HttpPost("/accounts/{accountname}/customer")]
        public async Task<IActionResult> CreateCustomer([FromRoute] string accountname, [FromBody] CustomerRequest customerRequest)
        {
            try
            {
                var customerResponse = await _cloverService.CreateCustomerAsync(accountname, customerRequest.Name, customerRequest.CreditCard);
                return Ok(customerResponse);
            }
            catch (CloverApiException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ApiError);
            }
        }

        [Authorize(Policies.OwnResourceOrSuperuser)]
        [HttpPut("/accounts/{accountname}/customer")]
        public async Task<IActionResult> UpdateCustomer([FromRoute] string accountname, [FromBody] CustomerRequest customerRequest)
        {
            try
            {
                var customer = await _cloverService.GetCustomerAsync(accountname);

                if (customer == null)
                {
                    return new NotFoundObjectResult("Invalid customer");
                }

                var cardId = customer.Cards.Elements.FirstOrDefault().Id;
                await _cloverService.RevokeCard(customerRequest.CloverCustomerId, cardId);

                var customerResponse = await _cloverService.UpdateCustomerAsync(accountname, customerRequest.Name, customerRequest.CreditCard, customerRequest.CloverCustomerId);
                return Ok(customerResponse);
            }
            catch (CloverApiException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ApiError);
            }
        }
    }
}
