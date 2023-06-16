using System.Collections.Generic;
using Doppler.CloverAPI.Entities.Clover;

namespace Doppler.CloverAPI.Response
{
    public class GetCustomerResponse
    {
        public IList<Customer> Elements { get; set; }
    }
}
