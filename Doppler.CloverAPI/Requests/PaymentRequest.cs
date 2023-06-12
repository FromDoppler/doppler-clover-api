using Doppler.CloverAPI.Entities;

namespace Doppler.CloverAPI.Requests
{
    public class PaymentRequest
    {
        public decimal ChargeTotal { get; set; }
        public CreditCard CreditCard { get; set; }
        public int ClientId { get; set; }
    }
}
