using System.Text.Json.Serialization;

namespace Doppler.CloverAPI.Requests
{
    public class CreateRefundRequest
    {
        public string Charge { get; set; }
        public int Amount { get; set; }

        [JsonPropertyName("external_reference_id")]
        public string ExternalReferenceId { get; set; }

        public string Reason { get; set; }
    }
}
