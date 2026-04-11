using System.Text.Json.Serialization;

namespace Microsoft.Maui.Media
{
    internal class TextToSpeechResponseInterop
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("lang")]
        public string? Lang { get; set; }

        [JsonPropertyName("default")]
        public bool Default { get; set; }

        [JsonPropertyName("voiceURI")]
        public string? VoiceURI { get; set; }

        [JsonPropertyName("localService")]
        public bool LocalService { get; set; }
    }
}
