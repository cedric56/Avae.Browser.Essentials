using System.Text.Json.Serialization;

namespace Microsoft.Maui.Devices
{
    internal class BatteryResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("charging")]
        public bool Charging { get; set; }

        [JsonPropertyName("chargingTime")]
        public double? ChargingTime { get; set; }

        [JsonPropertyName("dischargingTime")]
        public double? DischargingTime { get; set; }

        [JsonPropertyName("level")]
        public double Level { get; set; }

        [JsonPropertyName("errorCode")]
        public int? ErrorCode { get; set; }
    }
}
