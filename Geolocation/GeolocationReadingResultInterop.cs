using System.Text.Json.Serialization;

namespace Microsoft.Maui.Devices.Sensors
{
    internal class GeolocationReadingResultInterop
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("altitude")]
        public double? Altitude { get; set; }

        [JsonPropertyName("accuracy")]
        public double Accuracy { get; set; }

        [JsonPropertyName("altitudeAccuracy")]
        public double? AltitudeAccuracy { get; set; }

        [JsonPropertyName("speed")]
        public double? Speed { get; set; }

        [JsonPropertyName("heading")]
        public double? Course { get; set; }
    }
}
