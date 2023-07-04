using System.Text.Json.Serialization;

namespace Doppler.CloverAPI.Entities.Clover
{
    public class Charge
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("auth_code")]
        public string AuthorizationNumber { get; set; }
    }
}
