﻿using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices
{
    partial class VibrationImplementation : IVibration
    {
        [JSImport("vibrationInterop.cancel", "essentials")]
        public static partial void CancelVibrate();

        [JSImport("vibrationInterop.vibrate", "essentials")]
        public static partial void VibrateWithoutDuration();

        [JSImport("vibrationInterop.vibrateWithDuration", "essentials")]
        public static partial void VibrateWithDuration(int milliseconds);

        public bool IsSupported
            => true;

        void PlatformVibrate()
            => VibrateWithoutDuration();

        void PlatformVibrate(TimeSpan duration)
            => VibrateWithDuration(duration.Milliseconds);

        void PlatformCancel()
            => CancelVibrate();
    }
}
