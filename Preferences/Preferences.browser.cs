using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace Microsoft.Maui.Storage
{
    partial class PreferencesImplementation : IPreferences
    {
        [JSImport("preferencesInterop.setItem", "essentials")]
        public static partial void SetItem(string key, string value);

        [JSImport("preferencesInterop.getItem", "essentials")]
        public static partial string GetItem(string key);

        [JSImport("preferencesInterop.removeItem", "essentials")]
        public static partial bool RemoveItem(string key);

        [JSImport("preferencesInterop.clear", "essentials")]
        public static partial void ClearPreferences();


        public bool ContainsKey(string key, string sharedName)
        {
            return !string.IsNullOrEmpty(GetItem(key));
        }

        public void Remove(string key, string sharedName)
        {
            RemoveItem(key);
        }

        public void Clear(string sharedName)
        {
            ClearPreferences();
        }

        public void Set<T>(string key, T value, string sharedName)
        {
            //Preferences.CheckIsSupportedType<T>();

            SetItem(key, JsonSerializer.Serialize(value));
        }

        public T Get<T>(string key, T defaultValue, string sharedName)
        {
            var value = GetItem(key);
            if (value == null)
                return defaultValue;
            return JsonSerializer.Deserialize<T>(value);
        }
    }
}

