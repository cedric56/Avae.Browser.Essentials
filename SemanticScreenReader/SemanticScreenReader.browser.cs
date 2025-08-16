using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Accessibility
{
    partial class SemanticScreenReaderImplementation : ISemanticScreenReader
    {
        [JSImport("semanticScreenReaderInterop.announce", "essentials")]
        public static partial void AnnounceJs(string text);

        public void Announce(string text) =>
            AnnounceJs(text);
    }
}
