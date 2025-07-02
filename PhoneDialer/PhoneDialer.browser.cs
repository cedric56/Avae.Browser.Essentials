using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class PhoneDialerImplementation : IPhoneDialer
    {
        [JSImport("globalThis.eval")]
        public static partial Task Invoke(string @params);

        public bool IsSupported => true;

        public async void Open(string number)
        {
            ValidateOpen(number);

            await Invoke($"window.location.href='tel:{number}'");
        }
    }
}
