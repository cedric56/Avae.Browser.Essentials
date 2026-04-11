using Microsoft.Maui.Essentials;
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
            var info = JsonSerializer.Deserialize(json, AvaeJsonSerializerContext.Default.DeviceDisplayInfo);
            if(info == null)
                return new DisplayInfo(0, 0, 1.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            return new DisplayInfo(info.Width, info.Height, info.Density,
                info.IsLandscape ? DisplayOrientation.Landscape : DisplayOrientation.Portrait,
                info.Rotation == 0 ? DisplayRotation.Rotation0 :
                info.Rotation == 90 ? DisplayRotation.Rotation90 :
                info.Rotation == 180 ? DisplayRotation.Rotation180 :
                info.Rotation == 270 ? DisplayRotation.Rotation270 : DisplayRotation.Unknown);

        }

        protected override void StartScreenMetricsListeners()
        {
            
        }

        protected override void StopScreenMetricsListeners()
        {

        }
    }
}
