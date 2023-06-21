using Doppler.CloverAPI.Entities;

namespace Doppler.CloverAPI.Requests
{
    public class CustomerRequest
    {
        public string CloverCustomerId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public CreditCard CreditCard { get; set; }
    }
}
