using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Essentials
{    
    public partial class JSInterop
    {
        [JSImport("databaseInterop.fetch", "essentials")]
        public static partial string Fetch(string url, string data);

        [JSImport("globalThis.eval")]
        public static partial void Eval(string @params);

        [JSImport("globalThis.eval")]
        public static partial string Query(string @params);

        [JSImport("globalThis.open")]
        public static partial void Open(string url, string param = "_blank");
    }
}
