using Newtonsoft.Json;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Media
{
    partial class TextToSpeechImplementation : ITextToSpeech
    {
        [JSImport("textToSpeechInterop.speak", "essentials")]
        internal static partial void Speak(string text);

        [JSImport("textToSpeechInterop.speakWithOptions", "essentials")]
        internal static partial void SpeakWithOptions(string text, string lang, string voice, float? volume, float? pitch);

        [JSImport("textToSpeechInterop.getVoices", "essentials")]
        internal static partial Task<string> GetVoices();

        async Task<IEnumerable<Locale>> PlatformGetLocalesAsync()
        {
            var json = await GetVoices();
            var voices = JsonConvert.DeserializeObject<IEnumerable<Root>>(json);
            if(voices == null)
                return Enumerable.Empty<Locale>();
            var locales = voices.Select(v =>
            {
                int pFrom = v.name.LastIndexOf("(") + "(".Length;
                int pTo = v.name.LastIndexOf(")");

                var country = pTo - pFrom > -1 ? v.name.Substring(pFrom, pTo - pFrom) : string.Empty;

                var locale = new Locale(v.lang, country, v.name, v.voiceURI);
                return locale;
            }).ToList();
            return locales;
        }

        Task PlatformSpeakAsync(string text, SpeechOptions? options = null, CancellationToken cancelToken = default)
        {
            if (options == null)
            {
                Speak(text);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(options.Locale?.Language))
                    throw new ArgumentException("Locale language must be specified.", nameof(options.Locale));

                SpeakWithOptions(text,
                options.Locale.Language,
                options.Locale.Id,
                options.Volume,
                options.Pitch);
            }

            return Task.CompletedTask;
        }

        public class Root
        {
            public string name { get; set; }
            public string lang { get; set; }
            public bool @default { get; set; }
            public string voiceURI { get; set; }
            public bool localService { get; set; }
        }
    }
}
