using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace Microsoft.Maui.Devices
{
    partial class DeviceDisplayImplementation : IDeviceDisplay
    {
        [JSImport("deviceDisplayInterop.get", "essentials")]
        public static partial string GetDeviceDisplayInfo();

        [JSImport("deviceDisplayInterop.requestWakeLock", "essentials")]
        public static partial void RequestWakeLock();

        [JSImport("deviceDisplayInterop.releaseWakeLock", "essentials")]
        public static partial void ReleaseWakeLock();

        [JSExport]
        public static void OnDeviceDisplayChanged()
        {
            var instance = DeviceDisplay.Current as DeviceDisplayImplementation;
            if (instance == null)
                return;

            instance.OnMainDisplayInfoChanged();
        }

        bool keepScreenOn = false;

        protected override bool GetKeepScreenOn() => keepScreenOn;

        protected override void SetKeepScreenOn(bool keepScreenOn)
        {
            this.keepScreenOn = keepScreenOn;

            if(this.keepScreenOn)
            {
                RequestWakeLock();
            }
            else
            {
                ReleaseWakeLock();
            }
        }

        protected override DisplayInfo GetMainDisplayInfo()
        {
            var json = GetDeviceDisplayInfo();
            var info = JsonSerializer.Deserialize<DeviceDisplayInfo>(json);
            if(info == null)
                return new DisplayInfo(0, 0, 1.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            return new DisplayInfo(info.width, info.height, info.density,
                info.isLandscape ? DisplayOrientation.Landscape : DisplayOrientation.Portrait,
                info.rotation == 0 ? DisplayRotation.Rotation0 :
                info.rotation == 90 ? DisplayRotation.Rotation90 :
                info.rotation == 180 ? DisplayRotation.Rotation180 :
                info.rotation == 270 ? DisplayRotation.Rotation270 : DisplayRotation.Unknown);

        }

        protected override void StartScreenMetricsListeners()
        {
            
        }

        protected override void StopScreenMetricsListeners()
        {

        }

        public class DeviceDisplayInfo
        {
            public double width { get; set; }
            public double height { get; set; }
            public bool isLandscape { get; set; }
            public double density { get; set; } // Device Pixel Ratio
            public int rotation { get; set; } // Screen Rotation Angle (0, 90, 180, 270)
        }
    }
}
