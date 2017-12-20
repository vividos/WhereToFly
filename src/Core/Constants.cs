using System;

namespace WhereToFly.Core
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
        /// MessageCenter message constant to show toast message
        /// </summary>
        public const string MessageShowToast = "ShowToast";

        /// <summary>
        /// Page key for use in NavigationService, to navigate to settings page
        /// </summary>
        public const string PageKeyLocationDetailsPage = "LocationDetailsPage";

        /// <summary>
        /// Page key for use in NavigationService, to navigate to settings page
        /// </summary>
        public const string PageKeySettingsPage = "SettingsPage";

        /// <summary>
        /// Page key for use in NavigationService, to navigate to info page
        /// </summary>
        public const string PageKeyInfoPage = "InfoPage";

        /// <summary>
        /// Primary color for User Interface
        /// </summary>
        public static readonly Xamarin.Forms.Color PrimaryColor = Xamarin.Forms.Color.FromHex("2F299E");

        /// <summary>
        /// GeoLocation: Time span that must elapse until the next update is sent
        /// </summary>
        public static readonly TimeSpan GeoLocationMinimumTimeForUpdate = TimeSpan.FromSeconds(5);

        /// <summary>
        /// GeoLocation: Minimum distance to travel to send the next update
        /// </summary>
        public const double GeoLocationMinimumDistanceForUpdateInMeters = 2;
    }
}
