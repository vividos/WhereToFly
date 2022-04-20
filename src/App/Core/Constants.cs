using System;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core
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
        /// Visual Studio App Center key for UWP app
        /// </summary>
        public const string AppCenterKeyUwp = "74bfda82-7b61-4490-ac61-b28ec404c1fc";

        /// <summary>
        /// MessagingCenter message constant to show toast message
        /// </summary>
        public const string MessageShowToast = "ShowToast";

        /// <summary>
        /// MessagingCenter message constant to add tour plan location
        /// </summary>
        public const string MessageAddTourPlanLocation = "AddTourPlanLocation";

        /// <summary>
        /// MessagingCenter message constant to update settings on MapPage
        /// </summary>
        public const string MessageUpdateMapSettings = "UpdateMapSettings";

        /// <summary>
        /// GeoLocation: Minimum distance to travel to send the next update
        /// </summary>
        public const double GeoLocationMinimumDistanceForUpdateInMeters = 2;

        /// <summary>
        /// GeoLocation: Time span that must elapse until the next update is sent
        /// </summary>
        public static readonly TimeSpan GeoLocationMinimumTimeForUpdate = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Primary color for User Interface
        /// </summary>
        public static readonly Xamarin.Forms.Color PrimaryColor = Xamarin.Forms.Color.FromHex("2F299E");

        /// <summary>
        /// Initial center point for the map when no last know position is available
        /// </summary>
        public static readonly MapPoint InitialCenterPoint = new(47.67, 11.88);

        /// <summary>
        /// Bing maps API key; used for displaying Bing maps layer
        /// </summary>
        public static readonly string BingMapsKey = "AuuY8qZGx-LAeruvajcGMLnudadWlphUWdWb0k6N6lS2QUtURFk3ngCjIXqqFOoe";

        /// <summary>
        /// Bing maps API key; used for geocoding in UWP app
        /// </summary>
        public static readonly string BingMapsKeyUwp = "8KK08riht3IKUuEIHhO9~kiUuKLsWDkuE5jm4d2gBDQ~Aho0ktyzI5HmMR7lM_2JwyptBV5ltSyzJAgvfISK_RE1BexmpSH2bInepuuZrMjP";

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
