using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Microsoft.Maui.Devices;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.ApplicationModel
{
    partial class AppInfoImplementation : IAppInfo
    {
        const string SettingsUri = "intent://settings#Intent;scheme=package;package=com.android.browser;end";// ms-settings:appsfeatures-app";

        [JSImport("globalThis.eval")]
        public static partial string Eval(string @params);

        [JSImport("globalThis.eval")]
        public static partial void Invoke(string @params);

        public string PackageName => string.Empty;

        public string Name
        {
            get
            {
                string version = Eval("document.querySelector('meta[name=\"name\"]')?.content");
                if (string.IsNullOrEmpty(version))
                    Debug.WriteLine("Define a meta     <meta name=\"name\" content=\"App\" />\r\n");

                return version;
            }
        }

        public string VersionString
        {
            get
            {
                string version = Eval("document.querySelector('meta[name=\"app-version\"]')?.content");
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
                string version = Eval("document.querySelector('meta[name=\"build-version\"]')?.content");
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
                var direction = GetTopLevel().FlowDirection;
                if (direction == null)
                    return LayoutDirection.Unknown;
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
                    Invoke($"window.location.href='{SettingsUri}'");
                else if (!string.IsNullOrEmpty(implementation.Name))
                    Invoke($"window.location.href='{implementation.Name}://settings/'");
                //else if(implementation.RealPlatform == DevicePlatform.WinUI)
                //     Invoke($"window.location.href='ms-settings:appsfeatures'");
            }

            
        }

        public static TopLevel GetTopLevel()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return TopLevel.GetTopLevel(desktopLifetime.MainWindow);
            }
            else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                return TopLevel.GetTopLevel(singleViewPlatform.MainView);
            }

            return TopLevel.GetTopLevel(null);
        }
    }
}
