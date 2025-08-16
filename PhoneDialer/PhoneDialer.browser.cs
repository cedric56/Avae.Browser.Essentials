using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class PhoneDialerImplementation : IPhoneDialer
    {        
        public bool IsSupported => true;

        public void Open(string number)
        {
            ValidateOpen(number);

            JSInterop.Eval($"window.location.href='tel:{number}'");
        }
    }
}
