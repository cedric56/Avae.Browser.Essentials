using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Storage
{
    partial class SecureStorageImplementation : ISecureStorage, IPlatformSecureStorage
    {
        [JSImport("secureStorageInterop.setItem", "essentials")]
        public static partial void SetItem(string key, string value);

        [JSImport("secureStorageInterop.getItem", "essentials")]
        public static partial string GetItem(string key);
          
        [JSImport("secureStorageInterop.removeItem", "essentials")]
        public static partial bool RemoveItem(string key);

        [JSImport("secureStorageInterop.clear", "essentials")]
        public static partial void Clear();

        [JSImport("encrypt", "essentials")]
        public static partial Task<string> Encrypt(string key, string password);

        [JSImport("decrypt", "essentials")]
        public static partial Task<string> Decrypt(string key, string password);

        async Task<string> PlatformGetAsync(string key)
        {
            var item = GetItem(key);
            if (string.IsNullOrEmpty(item))
                return string.Empty;
            return await Decrypt(item,"abc123");
        }

        async Task PlatformSetAsync(string key, string data)
        {
            var item = await Encrypt(data, "abc123");
            SetItem(key, item);
        }

        bool PlatformRemove(string key)
        {
            return RemoveItem(key);
        }

        void PlatformRemoveAll()
        {
            Clear();
        }
    }
}
