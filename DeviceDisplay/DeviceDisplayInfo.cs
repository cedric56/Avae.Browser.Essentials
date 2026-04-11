using System.Text.Json.Serialization;

namespace Microsoft.Maui.Devices
{
    internal class DeviceDisplayInfo
    {
        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("isLandscape")]
        public bool IsLandscape { get; set; }

        [JsonPropertyName("density")]
        public double Density { get; set; } // Device Pixel Ratio

        [JsonPropertyName("rotation")]
        public int Rotation { get; set; } // Screen Rotation Angle (0, 90, 180, 270)
    }
}
