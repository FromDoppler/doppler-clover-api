using System.Text.Json.Serialization;

namespace Doppler.CloverAPI.Response
{
    public class CreateChargeResponse
    {
        public string Id { get; set; }
        [JsonPropertyName("auth_code")]
        public string AuthorizationNumber { get; set; }
    }
}
