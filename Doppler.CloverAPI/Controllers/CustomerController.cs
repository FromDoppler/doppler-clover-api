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
        private readonly IClientAddressService _clientAddressService;

        public CustomerController(ICloverService cloverService, IClientAddressService clientAddressService)
        {
            _cloverService = cloverService;
            _clientAddressService = clientAddressService;
        }

        [Authorize(Policies.OwnResourceOrSuperuser)]
        [HttpGet("/accounts/{accountname}/customer")]
        public async Task<IActionResult> GetCustomer([FromRoute] string accountname)
        {
            try
            {
                var clientIp = await _clientAddressService.GetIpAddress(accountname);
                var customerResponse = await _cloverService.GetCustomerAsync(accountname, clientIp);
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
                var clientIp = await _clientAddressService.GetIpAddress(accountname);
                var customerResponse = await _cloverService.CreateCustomerAsync(accountname, customerRequest.Name, customerRequest.CreditCard, clientIp);
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
                var clientIp = await _clientAddressService.GetIpAddress(accountname);
                var customer = await _cloverService.GetCustomerAsync(accountname, clientIp);

                if (customer == null)
                {
                    return new NotFoundObjectResult("Invalid customer");
                }

                var cardId = customer.Cards.Elements.FirstOrDefault().Id;
                await _cloverService.RevokeCard(customerRequest.CloverCustomerId, cardId, clientIp);

                var customerResponse = await _cloverService.UpdateCustomerAsync(accountname, customerRequest.Name, customerRequest.CreditCard, customerRequest.CloverCustomerId, clientIp);
                return Ok(customerResponse);
            }
            catch (CloverApiException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.ApiError);
            }
        }
    }
}
