using System.Text.Json.Serialization;

namespace Doppler.CloverAPI.Requests
{
    public class CreateChargeRequest
    {
        public int Amount { get; set; }
        public string Ecomind { get; set; }
        public string Currency { get; set; }
        [JsonPropertyName("external_reference_id")]
        public string ExternalReferenceId { get; set; }
        public bool Capture { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }

    }
}
