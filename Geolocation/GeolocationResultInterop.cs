using System.Text.Json.Serialization;

namespace Microsoft.Maui.Devices.Sensors
{
    internal class GeolocationResultInterop
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("accuracy")]
        public double? Accuracy { get; set; }

        [JsonPropertyName("errorCode")]
        public int? ErrorCode { get; set; }
    }
}
