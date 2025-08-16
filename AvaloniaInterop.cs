using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace Microsoft.Maui.Essentials
{
    internal class AvaloniaInterop
    {
        public static TopLevel? GetTopLevel()
        {
            if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime desktopLifetime)
            {
                return TopLevel.GetTopLevel(desktopLifetime.MainView);
            }

            return TopLevel.GetTopLevel(null);
        }

        /// <summary>
        /// Retrieves the storage provider from the current top-level window.
        /// </summary>
        public static IStorageProvider? GetStorage()
        {
            return GetTopLevel()?.StorageProvider;
        }

        public static FlowDirection? GetFlowDirection()
        {
            return GetTopLevel()?.FlowDirection;
        }
    }
}
