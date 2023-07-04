using Doppler.CloverAPI.Entities.Clover;

namespace Doppler.CloverAPI.Response
{
    public class CreateRefundResponse
    {
        public string Id { get; set; }

        public Refund Metadata { get; set; }
    }
}
