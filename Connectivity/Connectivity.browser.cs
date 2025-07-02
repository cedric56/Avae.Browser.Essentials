using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Networking
{
    public partial class ConnectivityImplementation : IConnectivity
    {
        [JSImport("connectivityInterop.isOnline", "essentials")]
        public static partial bool IsOnline();

        [JSImport("connectivityInterop.getConnectionType", "essentials")]
        public static partial string GetConnectionType();

        private void RaiseConnectivityChanged()
        {
            if (!_isMonitoring)
                return;

            OnConnectivityChanged();
        }

        [JSExport]
        public static void OnConnectivityChanged(string value)
        {
            if (Connectivity.Current is not ConnectivityImplementation implementation)
                return;
            implementation.Initialize();
            implementation.RaiseConnectivityChanged();
        }

        private bool _isMonitoring;
        void StartListeners()
        {
            _isMonitoring = true;
        }

        void StopListeners()
        {
            _isMonitoring = false;
        }

        public IEnumerable<ConnectionProfile>? ConnectionProfiles { get; set; }

        public NetworkAccess NetworkAccess { get; set; }

        public void Initialize()
        {
            bool isOnline = IsOnline();
            NetworkAccess = isOnline ? NetworkAccess.Internet : NetworkAccess.None;

            var profiles = new List<ConnectionProfile>();
            if (isOnline)
            {
                var networkType = GetConnectionType();
                if (Enum.TryParse(networkType, true, out ConnectionProfile profile))
                    profiles.Add(profile);
                else if (!string.IsNullOrEmpty(networkType))
                    profiles.Add(ConnectionProfile.Cellular);
            }
            if (profiles.Count == 0) profiles.Add(ConnectionProfile.Unknown);
            ConnectionProfiles = profiles;
        }
    }
}
