using System.Text.Json.Serialization;

namespace Microsoft.Maui.Devices.Sensors
{
    // {"timestamp":1775912506229,"coords":{"accuracy":103,"latitude":43.34419819928297,"longitude":132.1563795,"altitude":null,"altitudeAccuracy":null,"heading":null,"speed":null}}
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
