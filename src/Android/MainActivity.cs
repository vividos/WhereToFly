using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Plugin.Permissions;
using WhereToFly.Core;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace WhereToFly.Android
{
    /// <summary>
    /// Main activity for the Android app
    /// </summary>
    [Activity(Label = Constants.AppTitle,
        Name = "wheretofly.MainActivity",
        Icon = "@drawable/icon",
        Theme = "@style/MainTheme",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
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

            Forms.SetFlags("FastRenderers_Experimental");
            Forms.Init(this, savedInstanceState);

            MessagingCenter.Subscribe<App, string>(this, Constants.MessageShowToast, this.ShowToast);

            this.LoadApplication(new Core.App());
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
            global::Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // let Plugin.Permissions handle the request
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// Shows toast message with given text
        /// </summary>
        /// <param name="app">app object; unused</param>
        /// <param name="message">toast message</param>
        private void ShowToast(App app, string message)
        {
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }
    }
}
