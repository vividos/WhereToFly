using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Plugin.Permissions;
using System.IO;
using WhereToFly.App.Core;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace WhereToFly.App.Android
{
    /// <summary>
    /// Main activity for the Android app.
    /// The attributes specify startup behavior. LaunchMode.SingleTask is used when the activity
    /// is started again with an intent to open files. See
    /// http://www.helloandroid.com/tutorials/communicating-between-running-activities
    /// The intent filters are used to open various combinations of files.
    /// </summary>
    [Activity(Label = Constants.AppTitle,
        Name = "wheretofly.MainActivity",
        Icon = "@drawable/icon",
        Theme = "@style/MainTheme",
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    //// Intent filter, case 1: mime type set
    //// See https://stackoverflow.com/questions/39300649/android-intent-filter-not-working
    [IntentFilter(
        new[] { Intent.ActionView, Intent.ActionOpenDocument },
        DataSchemes = new string[] { "file", "content", "http", "https" },
        DataMimeTypes = new string[]
        {
            "application/vnd.google-earth.kml+xml",
            "application/vnd.google-earth.kmz",
            "application/gpx+xml",
        },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        Icon = "@drawable/icon")]
    //// Intent filter, case 2: mime type not set, but valid extensions
    [IntentFilter(
        new[] { Intent.ActionView, Intent.ActionOpenDocument },
        DataSchemes = new string[] { "file", "content", "http", "https" },
        DataHost = "*",
        DataPathPatterns = new string[]
        {
            @".*\\.kmz", @".*\\..*\\.kmz", ".*\\..*\\..*\\.kmz", ".*\\..*\\..*\\..*\\.kmz",
            @".*\\.kml", @".*\\..*\\.kml", ".*\\..*\\..*\\.kml", ".*\\..*\\..*\\..*\\.kml",
            @".*\\.gpx", @".*\\..*\\.gpx", ".*\\..*\\..*\\.gpx", ".*\\..*\\..*\\..*\\.gpx",
            @".*\\.igc", @".*\\..*\\.igc", ".*\\..*\\..*\\.igc", ".*\\..*\\..*\\..*\\.igc",
            @".*\\.czml", @".*\\..*\\.czml", ".*\\..*\\..*\\.czml", ".*\\..*\\..*\\..*\\.czml",
        },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        Icon = "@drawable/icon")]
    //// Intent filter, case 3: application/octet-stream, and valid extension
    [IntentFilter(
        new[] { Intent.ActionView, Intent.ActionOpenDocument },
        DataSchemes = new string[] { "file", "content", "http", "https" },
        DataHost = "*",
        DataMimeType = "application/octet-stream",
        DataPathPatterns = new string[]
        {
            @".*\\.kmz", @".*\\..*\\.kmz", ".*\\..*\\..*\\.kmz", ".*\\..*\\..*\\..*\\.kmz",
            @".*\\.kml", @".*\\..*\\.kml", ".*\\..*\\..*\\.kml", ".*\\..*\\..*\\..*\\.kml",
            @".*\\.gpx", @".*\\..*\\.gpx", ".*\\..*\\..*\\.gpx", ".*\\..*\\..*\\..*\\.gpx",
            @".*\\.igc", @".*\\..*\\.igc", ".*\\..*\\..*\\.igc", ".*\\..*\\..*\\..*\\.igc",
            @".*\\.czml", @".*\\..*\\.czml", ".*\\..*\\..*\\.czml", ".*\\..*\\..*\\..*\\.czml",
        },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        Icon = "@drawable/icon")]
    //// Intent filter, case 4: application/octet-stream, and valid extension, but not data host
    [IntentFilter(
        new[] { Intent.ActionView, Intent.ActionOpenDocument, Intent.ActionDefault },
        DataSchemes = new string[] { "content" },
        DataMimeType = "application/octet-stream",
        DataPathPatterns = new string[]
        {
            @".*\\.kmz", @".*\\..*\\.kmz", ".*\\..*\\..*\\.kmz", ".*\\..*\\..*\\..*\\.kmz",
            @".*\\.kml", @".*\\..*\\.kml", ".*\\..*\\..*\\.kml", ".*\\..*\\..*\\..*\\.kml",
            @".*\\.gpx", @".*\\..*\\.gpx", ".*\\..*\\..*\\.gpx", ".*\\..*\\..*\\..*\\.gpx",
            @".*\\.igc", @".*\\..*\\.igc", ".*\\..*\\..*\\.igc", ".*\\..*\\..*\\..*\\.igc",
            @".*\\.czml", @".*\\..*\\.czml", ".*\\..*\\..*\\.czml", ".*\\..*\\..*\\..*\\.czml",
        },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        Icon = "@drawable/icon")]
    //// Intent filter, case 5: URI with where-to-fly scheme
    [IntentFilter(
        new[] { Intent.ActionView },
        DataScheme = WhereToFly.Shared.Model.AppResourceUri.DefaultScheme,
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        Icon = "@drawable/icon")]
    public class MainActivity : FormsAppCompatActivity
    {
        /// <summary>
        /// Called in the activity lifecycle when the activity is about to be created. This starts
        /// the Xamarin.Forms based app
        /// </summary>
        /// <param name="savedInstanceState">bundle parameter; unused</param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            FormsAppCompatActivity.TabLayoutResource = Resource.Layout.Tabbar;
            FormsAppCompatActivity.ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);

            Forms.SetFlags("FastRenderers_Experimental");
            Forms.Init(this, savedInstanceState);

            FFImageLoading.ImageService.Instance.Initialize();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);

            MessagingCenter.Subscribe<Core.App, string>(this, Constants.MessageShowToast, this.ShowToast);

            this.LoadApplication(new Core.App());
        }

        /// <summary>
        /// Called when back button has been pressed; let the Rg.Plugins.Popup package handle back
        /// navigation first.
        /// </summary>
        public override void OnBackPressed()
        {
            if (!Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                base.OnBackPressed();
            }
        }

        /// <summary>
        /// Called when activity is called with a new intent, e.g. from the intent filter for file
        /// extension (.kml, .kmz, .gpx, .igc).
        /// See: https://stackoverflow.com/questions/3760276/android-intent-filter-associate-app-with-file-extension
        /// </summary>
        /// <param name="intent">intent to be passed to the app</param>
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            System.Diagnostics.Debug.WriteLine("received Intent: " + intent.ToString());

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
            var app = Core.App.Current as Core.App;
            if (intent.DataString != null &&
                intent.DataString.StartsWith(Shared.Model.AppResourceUri.DefaultScheme))
            {
                Core.App.RunOnUiThread(async () => await app.OpenAppResourceUriAsync(intent.DataString));
                return;
            }

            var helper = new IntentFilterHelper(this.ContentResolver);

            string filename = Path.GetFileName(helper.GetFilenameFromIntent(intent));
            if (filename == null)
            {
                return;
            }

            var stream = helper.GetStreamFromIntent(intent);

            if (stream != null)
            {
                Core.App.RunOnUiThread(async () => await app.OpenFileAsync(stream, filename));
            }
        }

        /// <summary>
        /// Called when a permissions request result has been sent to the activity
        /// </summary>
        /// <param name="requestCode">request code</param>
        /// <param name="permissions">list of permissions</param>
        /// <param name="grantResults">list of grant results</param>
        public override void OnRequestPermissionsResult(
            int requestCode,
            string[] permissions,
            Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // let Plugin.Permissions handle the request
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// Shows toast message with given text
        /// </summary>
        /// <param name="app">app object; unused</param>
        /// <param name="message">toast message</param>
        private void ShowToast(Core.App app, string message)
        {
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }
    }
}
