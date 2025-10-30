using Android.App;
using Android.Content;
using Android.Content.PM;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Logic;
using WhereToFly.Geo;

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace WhereToFly.App
{
    /// <summary>
    /// Android MAUI app main activity.
    /// The attributes specify startup behavior. LaunchMode.SingleTask is used when the activity
    /// is started again with an intent to open files. See
    /// https://web.archive.org/web/20230323234840/http://www.helloandroid.com/tutorials/communicating-between-running-activities
    /// The intent filters are used to open various combinations of files.
    /// </summary>
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    //// Intent filter, case 1: mime type set
    //// See https://stackoverflow.com/questions/39300649/android-intent-filter-not-working
    [IntentFilter(
        [Intent.ActionView, Intent.ActionOpenDocument],
        DataSchemes = ["file", "content", "http", "https"],
        DataMimeTypes =
        [
            "application/vnd.google-earth.kml+xml",
            "application/vnd.google-earth.kmz",
            "application/gpx+xml",
            "application/x-igc", // xcontest .igc files return this MIME type
            "text/plain",
        ],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        Icon = "@mipmap/appicon")]
    //// Intent filter, case 2: mime type not set, but valid extensions
    [IntentFilter(
        [Intent.ActionView, Intent.ActionOpenDocument],
        DataSchemes = ["file", "content", "http", "https"],
        DataHost = "*",
        DataPathPatterns =
        [
            @".*\\.kmz", @".*\\..*\\.kmz", ".*\\..*\\..*\\.kmz", ".*\\..*\\..*\\..*\\.kmz",
            @".*\\.kml", @".*\\..*\\.kml", ".*\\..*\\..*\\.kml", ".*\\..*\\..*\\..*\\.kml",
            @".*\\.gpx", @".*\\..*\\.gpx", ".*\\..*\\..*\\.gpx", ".*\\..*\\..*\\..*\\.gpx",
            @".*\\.igc", @".*\\..*\\.igc", ".*\\..*\\..*\\.igc", ".*\\..*\\..*\\..*\\.igc",
            @".*\\.czml", @".*\\..*\\.czml", ".*\\..*\\..*\\.czml", ".*\\..*\\..*\\..*\\.czml",
            @".*\\.cup", @".*\\..*\\.cup", ".*\\..*\\..*\\.cup", ".*\\..*\\..*\\..*\\.cup",
            @".*\\.txt", @".*\\..*\\.txt", ".*\\..*\\..*\\.txt", ".*\\..*\\..*\\..*\\.txt",
        ],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        Icon = "@mipmap/appicon")]
    //// Intent filter, case 3: application/octet-stream, and valid extension
    [IntentFilter(
        [Intent.ActionView, Intent.ActionOpenDocument],
        DataSchemes = ["file", "content", "http", "https"],
        DataHost = "*",
        DataMimeType = "application/octet-stream",
        DataPathPatterns =
        [
            @".*\\.kmz", @".*\\..*\\.kmz", ".*\\..*\\..*\\.kmz", ".*\\..*\\..*\\..*\\.kmz",
            @".*\\.kml", @".*\\..*\\.kml", ".*\\..*\\..*\\.kml", ".*\\..*\\..*\\..*\\.kml",
            @".*\\.gpx", @".*\\..*\\.gpx", ".*\\..*\\..*\\.gpx", ".*\\..*\\..*\\..*\\.gpx",
            @".*\\.igc", @".*\\..*\\.igc", ".*\\..*\\..*\\.igc", ".*\\..*\\..*\\..*\\.igc",
            @".*\\.czml", @".*\\..*\\.czml", ".*\\..*\\..*\\.czml", ".*\\..*\\..*\\..*\\.czml",
            @".*\\.cup", @".*\\..*\\.cup", ".*\\..*\\..*\\.cup", ".*\\..*\\..*\\..*\\.cup",
            @".*\\.txt", @".*\\..*\\.txt", ".*\\..*\\..*\\.txt", ".*\\..*\\..*\\..*\\.txt",
        ],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        Icon = "@mipmap/appicon")]
    //// Intent filter, case 4: application/octet-stream, and valid extension, but not data host
    [IntentFilter(
        [Intent.ActionView, Intent.ActionOpenDocument, Intent.ActionDefault],
        DataSchemes = ["content"],
        DataMimeType = "application/octet-stream",
        DataPathPatterns =
        [
            @".*\\.kmz", @".*\\..*\\.kmz", ".*\\..*\\..*\\.kmz", ".*\\..*\\..*\\..*\\.kmz",
            @".*\\.kml", @".*\\..*\\.kml", ".*\\..*\\..*\\.kml", ".*\\..*\\..*\\..*\\.kml",
            @".*\\.gpx", @".*\\..*\\.gpx", ".*\\..*\\..*\\.gpx", ".*\\..*\\..*\\..*\\.gpx",
            @".*\\.igc", @".*\\..*\\.igc", ".*\\..*\\..*\\.igc", ".*\\..*\\..*\\..*\\.igc",
            @".*\\.czml", @".*\\..*\\.czml", ".*\\..*\\..*\\.czml", ".*\\..*\\..*\\..*\\.czml",
            @".*\\.cup", @".*\\..*\\.cup", ".*\\..*\\..*\\.cup", ".*\\..*\\..*\\..*\\.cup",
            @".*\\.txt", @".*\\..*\\.txt", ".*\\..*\\..*\\.txt", ".*\\..*\\..*\\..*\\.txt",
        ],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        Icon = "@mipmap/appicon")]
    //// Intent filter, case 5: URI with where-to-fly scheme
    [IntentFilter(
        [Intent.ActionView],
        DataScheme = WhereToFly.Shared.Model.AppResourceUri.DefaultScheme,
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        Icon = "@mipmap/appicon")]
    //// Intent filter, case 6: geo: scheme, mime type not set
    [IntentFilter(
        [Intent.ActionView],
        DataSchemes = ["geo"],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        Icon = "@mipmap/appicon")]
    public class MainActivity : MauiAppCompatActivity
    {
        /// <summary>
        /// Called when activity is called with a new intent, e.g. from the intent filter for file
        /// extension (.kml, .kmz, .gpx, .igc, .czml, .cup, .txt).
        /// See: https://stackoverflow.com/questions/3760276/android-intent-filter-associate-app-with-file-extension
        /// </summary>
        /// <param name="intent">intent to be passed to the app</param>
        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            System.Diagnostics.Debug.WriteLine("received Intent: " + intent?.ToString());

            this.Intent = intent;
        }

        /// <summary>
        /// Called when activity is about to be resumed; this is called after OnCreate or
        /// OnNewIntent in order to check for a new file to open.
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            if (this.Intent != null)
            {
                this.ProcessIntent(this.Intent);
                this.Intent = null;
            }
        }

        /// <summary>
        /// Processes the given intent, loading files passed using content resolver.
        /// </summary>
        /// <param name="intent">intent to process</param>
        private void ProcessIntent(Intent intent)
        {
            try
            {
                if (intent.DataString != null &&
                    intent.DataString.StartsWith(Shared.Model.AppResourceUri.DefaultScheme))
                {
                    var appMapService = App.Services.GetRequiredService<IAppMapService>();
                    appMapService.OpenAppResourceUri(intent.DataString);
                    return;
                }

                if (intent.DataString != null &&
                    intent.DataString.StartsWith("geo:") &&
                    CoordinatesParser.TryParse(intent.DataString, out Geo.Model.MapPoint? mapPoint) &&
                    mapPoint != null)
                {
                    var appMapService = App.Services.GetRequiredService<IAppMapService>();
                    appMapService.AddNewLocation(mapPoint);
                    return;
                }

                var helper = new IntentFilterHelper(this.ContentResolver);

                string? filename = Path.GetFileName(helper.GetFilenameFromIntent(intent));
                if (filename == null)
                {
                    return;
                }

                Stream? stream = helper.GetStreamFromIntent(intent);

                if (stream != null)
                {
                    MainThread.BeginInvokeOnMainThread(
                        async () => await OpenFileHelper.OpenFileAsync(stream, filename));
                }
            }
            catch (Exception)
            {
                // ignore errors
            }
        }
    }
}
