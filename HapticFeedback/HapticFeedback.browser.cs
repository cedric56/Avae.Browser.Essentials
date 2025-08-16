using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Devices
{
    partial class HapticFeedbackImplementation : IHapticFeedback
    {
        [JSImport("vibrationInterop.vibrateWithDuration", "essentials")]
        public static partial void VibrateWithDuration(int milliseconds);

        public bool IsSupported => true;

        public void Perform(HapticFeedbackType type)
        {
            VibrateWithDuration(GetDuration(type));
        }

        private int GetDuration(HapticFeedbackType type)
        {
            if (type == HapticFeedbackType.Click)
                return 20;
            return 100;
        }
    }
}
