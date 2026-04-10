using System.Text.Json.Serialization;

namespace Microsoft.Maui.Devices.Sensors
{
    internal class Address
    {
        [JsonPropertyName("road")]
        public string? Road { get; set; }

        [JsonPropertyName("hamlet")]
        public string? Hamlet { get; set; }

        [JsonPropertyName("village")]
        public string? Village { get; set; }

        [JsonPropertyName("municipality")]
        public string? Municipality { get; set; }

        [JsonPropertyName("county")]
        public string? County { get; set; }

        [JsonPropertyName("ISO3166-2-lvl6")]
        public string? ISO31662lvl6 { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("ISO3166-2-lvl4")]
        public string? ISO31662lvl4 { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("country_code")]
        public string? CcountryCode { get; set; }
    }
}
