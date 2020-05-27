using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace PulseCheck.Utilities
{
    public class Settings
    {
        private static MemoryCache cachedSettings = MemoryCache.Default;

        /// <summary>
        /// Get the value of a specific setting
        /// </summary>
        /// <param name="setting">Setting whose value is wanted</param>
        /// <returns>A string of the setting's value.  Empty string if the setting doesn't exist</returns>
        public static string GetSetting(string setting)
        {
            var res = cachedSettings.Get("settings");
            if (res == null)
            {
                LoadSettings();
                res = cachedSettings.Get("settings");
            }
            var dict = (Dictionary<string, string>)res;
            return dict.ContainsKey(setting) ? dict[setting] : "";
        }

        /// <summary>
        /// Create a new Settings object 
        /// </summary>
        private static void LoadSettings()
        {
            var settings = new DB.Select
            {
                Sql = "SELECT setting_key, setting_value FROM master_settings"
            }.RunForDictionary("setting_key", "setting_value");

            cachedSettings.Add("settings", settings, DateTimeOffset.UtcNow.AddDays(7));
        }

        /// <summary>
        /// Set of settings constants
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// Minimum length for a password
            /// </summary>
            public const string PASSWORD_MINIMUM_LENGTH = "password_min_length";

            /// <summary>
            /// Minimum number of special characters a password should have
            /// </summary>
            public const string PASSWORD_MINIMUM_SPECIAL_CHARS = "password_min_special_chars";

            /// <summary>
            /// Minimum number of capital letters a password should have
            /// </summary>
            public const string PASSWORD_MINIMUM_CAPS = "password_min_caps";

            /// <summary>
            /// Minimum number of numbers a password should have
            /// </summary>
            public const string PASSWORD_MINIMUM_NUMBERS = "password_min_numbers";
        }
    }
}
