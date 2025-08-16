using Avalonia;
using Avalonia.Styling;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Essentials;
using System.Diagnostics;

namespace Microsoft.Maui.ApplicationModel
{
    partial class AppInfoImplementation : IAppInfo
    {
        const string SettingsUri = "intent://#Intent;component=com.android.browser/com.android.browser.BrowserPreferencesActivity;end";// ms-settings:appsfeatures-app";

        public string PackageName
        {
            get
            {
                string name = JSInterop.Query("document.querySelector('meta[name=\"package\"]')?.content");
                if (string.IsNullOrEmpty(name))
                    Debug.WriteLine("Define a meta     <meta name=\"package\" content=\"PackageName\" />\r\n");

                return name;
            }
        }

        public string Name
        {
            get
            {
                string version = JSInterop.Query("document.querySelector('meta[name=\"name\"]')?.content");
                if (string.IsNullOrEmpty(version))
                    Debug.WriteLine("Define a meta     <meta name=\"name\" content=\"App\" />\r\n");

                return version;
            }
        }

        public string VersionString
        {
            get
            {
                string version = JSInterop.Query("document.querySelector('meta[name=\"app-version\"]')?.content");
                if (string.IsNullOrEmpty(version))
                    Debug.WriteLine("Define a meta     <meta name=\"app-version\" content=\"1.2.3\" />\r\n");

                return version;
            }
        }

        public Version Version => new Version(VersionString);

        public string BuildString
        {
            get
            {
                string version = JSInterop.Query("document.querySelector('meta[name=\"build-version\"]')?.content");
                if (string.IsNullOrEmpty(version))
                    Debug.WriteLine("Define a meta     <meta name=\"build-version\" content=\"1.2.3\" />\r\n");

                return version;
            }
        }

        public AppTheme RequestedTheme
        {
            get
            {
                var theme = Application.Current?.ActualThemeVariant;
                if(theme == null)
                    return AppTheme.Unspecified;

                return theme == ThemeVariant.Light
                    ? AppTheme.Light
                    : AppTheme.Dark;
            }
        }

        public AppPackagingModel PackagingModel => AppPackagingModel.Unpackaged;

        public LayoutDirection RequestedLayoutDirection
        {
            get
            {
                var direction = AvaloniaInterop.GetFlowDirection();
                return direction == Avalonia.Media.FlowDirection.LeftToRight
                    ? LayoutDirection.LeftToRight
                    : LayoutDirection.RightToLeft;
            }
        }

        public  void ShowSettingsUI()
        {
            if (DeviceInfo.Current is DeviceInfoImplementation implementation)
            {
                if (implementation.RealPlatform == DevicePlatform.Android)
                    JSInterop.Eval($"window.location.href='{SettingsUri}'");
                else if (implementation.RealPlatform == DevicePlatform.WinUI)
                    JSInterop.Eval($"window.location.href='ms-settings:appsfeatures'");
                else if (implementation.RealPlatform == DevicePlatform.Linux)
                {
                    JSInterop.Eval($"window.location.href='gnome-control-center:applications'");
                }
            }
        }
    }
}
