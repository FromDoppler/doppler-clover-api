using System.Text.Json.Serialization;

namespace Doppler.CloverAPI.Entities.Clover
{
    public class CustomerCard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("expirationDate")]
        public string ExpirationDate { get; set; }

        [JsonPropertyName("last4")]
        public string Last4 { get; set; }

        [JsonPropertyName("first6")]
        public string First6 { get; set; }

        [JsonPropertyName("cardType")]
        public string CardType { get; set; }
    }
}
