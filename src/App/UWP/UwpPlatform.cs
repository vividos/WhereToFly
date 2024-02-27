using System;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.UWP.UwpPlatform))]

namespace WhereToFly.App.UWP
{
    /// <summary>
    /// Platform specific functions
    /// </summary>
    public class UwpPlatform : IPlatform
    {
        /// <summary>
        /// Base path to use in WebView control, for UWP
        /// </summary>
        public string WebViewBasePath => "ms-appx-web:///WhereToFly.App.MapView/Assets/";

        /// <summary>
        /// Sets app theme to use for platform. This ensures that platform dependent dialogs are
        /// themed correctly when switching themes.
        /// </summary>
        /// <param name="requestedTheme">requested theme</param>
        public void SetPlatformTheme(OSAppTheme requestedTheme)
        {
            // switch to UI thread; or else accessing RequestedTheme on UWP crashes
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => this.SetPlatformTheme(requestedTheme));
                return;
            }

            try
            {
                switch (requestedTheme)
                {
                    case OSAppTheme.Dark: App.Current.RequestedTheme = Windows.UI.Xaml.ApplicationTheme.Dark; break;
                    case OSAppTheme.Light: App.Current.RequestedTheme = Windows.UI.Xaml.ApplicationTheme.Light; break;
                    default:
                        // ignore other requested themes
                        break;
                }
            }
            catch (Exception)
            {
                // ignore errors when setting theme
            }
        }

        /// <summary>
        /// Translates the compass' magnetic north heading (e.g. from Xamarin.Essentials.Compass
        /// API) to true north.
        /// </summary>
        /// <param name="headingMagneticNorthInDegrees">magnetic north heading</param>
        /// <param name="latitudeInDegrees">latitude of current position</param>
        /// <param name="longitudeInDegrees">longitude of current position</param>
        /// <param name="altitudeInMeter">altitude of current position</param>
        /// <param name="headingTrueNorthInDegrees">true north heading</param>
        /// <returns>true when tralslating was successful, false when not available</returns>
        public bool TranslateCompassMagneticNorthToTrueNorth(
            int headingMagneticNorthInDegrees,
            double latitudeInDegrees,
            double longitudeInDegrees,
            double altitudeInMeter,
            out int headingTrueNorthInDegrees)
        {
            // on UWP this isn't possible. Xamarin.Essentials.Compass could return true north
            // heading directly in the CompassData, but doesn't.
            headingTrueNorthInDegrees = 0;
            return false;
        }
    }
}
