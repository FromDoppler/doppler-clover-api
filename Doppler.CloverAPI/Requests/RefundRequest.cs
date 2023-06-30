namespace Doppler.CloverAPI.Requests
{
    public class RefundRequest
    {
        public string ClientId { get; set; }
        public string ChargeAuthorizationNumber { get; set; }
        public decimal ChargeTotal { get; set; }

    }
}
