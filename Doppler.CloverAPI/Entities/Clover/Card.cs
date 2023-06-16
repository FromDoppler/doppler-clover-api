using System.Text.Json.Serialization;

namespace Doppler.CloverAPI.Entities.Clover
{
    public class Card
    {
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("exp_month")]
        public string ExpMonth { get; set; }

        [JsonPropertyName("exp_year")]
        public string ExpYear { get; set; }

        [JsonPropertyName("cvv")]
        public string Cvv { get; set; }

        [JsonPropertyName("last4")]
        public string Last4 { get; set; }

        [JsonPropertyName("first6")]
        public string First6 { get; set; }

        [JsonPropertyName("brand")]
        public string Brand { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
