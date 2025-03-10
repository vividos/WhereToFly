﻿using Microsoft.Maui.Devices.Sensors;
using WhereToFly.Geo.Model;

namespace WhereToFly.App
{
    /// <summary>
    /// Constants for the app
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// App title for display in pages and alert boxes
        /// </summary>
        public const string AppTitle = "Where-to-fly";

        /// <summary>
        /// Visual Studio App Center key for Android app
        /// </summary>
        public const string AppCenterKeyAndroid = "dc3a41ea-d024-41a8-9940-4529a24086b1";

        /// <summary>
        /// Visual Studio App Center key for Windows app
        /// </summary>
        public const string AppCenterKeyWindows = "74bfda82-7b61-4490-ac61-b28ec404c1fc";

        /// <summary>
        /// GeoLocation: Accuracy to use when listening for changes
        /// </summary>
        public const GeolocationAccuracy GeoLocationAccuracy = GeolocationAccuracy.Best;

        /// <summary>
        /// GeoLocation: Time span that must elapse until the next update is sent
        /// </summary>
        public static readonly TimeSpan GeoLocationMinimumTimeForUpdate = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Primary color for User Interface
        /// </summary>
        public static readonly Color PrimaryColor = Color.FromArgb("2F299E");

        /// <summary>
        /// Accent color for User Interface
        /// </summary>
        public static readonly Color AccentColor = Color.FromArgb("45C8CF");

        /// <summary>
        /// Initial center point for the map when no last know position is available
        /// </summary>
        public static readonly MapPoint InitialCenterPoint = new(47.67, 11.88);

        /// <summary>
        /// Bing maps API key; used for geocoding in Windows app
        /// </summary>
        public static readonly string BingMapsKeyWindows = "8KK08riht3IKUuEIHhO9~kiUuKLsWDkuE5jm4d2gBDQ~Aho0ktyzI5HmMR7lM_2JwyptBV5ltSyzJAgvfISK_RE1BexmpSH2bInepuuZrMjP";

        /// <summary>
        /// Key for the SecureStorage to store and read username for Alptherm web page
        /// </summary>
        public static readonly string SecureSettingsAlpthermUsername = "AlpthermUsername";

        /// <summary>
        /// Key for the SecureStorage to store and read password for Alptherm web page
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Sonar Vulnerability",
            "S2068:Hard-coded credentials are security-sensitive",
            Justification = "false positive")]
        public static readonly string SecureSettingsAlpthermPassword = "AlpthermPassword";
    }
}
