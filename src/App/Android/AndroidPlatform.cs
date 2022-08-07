using AndroidX.AppCompat.App;
using System;
using System.IO;
using WhereToFly.App.Core;
using Xamarin.Forms;

[assembly: Dependency(typeof(WhereToFly.App.Android.AndroidPlatform))]

namespace WhereToFly.App.Android
{
    /// <summary>
    /// Platform specific functions
    /// </summary>
    public class AndroidPlatform : IPlatform
    {
        /// <summary>
        /// Base path to use in WebView control, for Android
        /// </summary>
        public string WebViewBasePath => "file:///android_asset/";

        /// <summary>
        /// Opens asset stream and returns it
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>stream to read from file</returns>
        public Stream OpenAssetStream(string assetFilename)
        {
            var assetManager = global::Android.App.Application.Context.Assets;

            return assetManager.Open(assetFilename);
        }

        /// <summary>
        /// Loads text of Android asset file from given filename
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>text content of asset</returns>
        public string LoadAssetText(string assetFilename)
        {
            using var stream = this.OpenAssetStream(assetFilename);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Sets app theme to use for platform. This ensures that platform dependent dialogs are
        /// themed correctly when switching themes.
        /// </summary>
        /// <param name="requestedTheme">requested theme</param>
        public void SetPlatformTheme(OSAppTheme requestedTheme)
        {
            AppCompatDelegate.DefaultNightMode = requestedTheme switch
            {
                OSAppTheme.Dark => AppCompatDelegate.ModeNightYes,
                OSAppTheme.Light => AppCompatDelegate.ModeNightNo,
                _ => AppCompatDelegate.ModeNightFollowSystem,
            };
        }

        /// <summary>
        /// Translates the compass' magnetic north heading (e.g. from Xamarin.Essentials.Compass
        /// API) to true north. On Android this is done using the GeomagneticField class that
        /// returns the magnetic declination on the given coordinates
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
            long millisecondsSinceEpoch = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var geomagneticField = new global::Android.Hardware.GeomagneticField(
                (float)latitudeInDegrees,
                (float)longitudeInDegrees,
                (float)altitudeInMeter,
                millisecondsSinceEpoch);

            headingTrueNorthInDegrees =
                (int)(headingMagneticNorthInDegrees + geomagneticField.Declination);

            return true;
        }
    }
}
