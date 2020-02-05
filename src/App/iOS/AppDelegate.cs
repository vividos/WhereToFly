using Foundation;
using UIKit;
using WhereToFly.App.Core;
using Xamarin.Forms;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.StyleCop.CSharp.NamingRules",
    "SA1300:ElementMustBeginWithUpperCaseLetter",
    Scope = "namespace",
    Target = "WhereToFly.App.iOS",
    Justification = "iOS is a proper name")]

namespace WhereToFly.App.iOS
{
    /// <summary>
    /// The UIApplicationDelegate for the application. This class is responsible for launching the 
    /// User Interface of the application, as well as listening (and optionally responding) to 
    /// application events from iOS.
    /// </summary>
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        /// <summary>
        /// This method is invoked when the application has loaded and is ready to run. In this 
        /// method you should instantiate the window, load the UI into it and then make the window
        /// visible.
        /// You have 17 seconds to return from this method, or iOS will terminate your application.
        /// </summary>
        /// <param name="uiApplication">application object</param>
        /// <param name="launchOptions">event options</param>
        /// <returns>indicates if launching has finished successfully</returns>
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
#if ENABLE_TEST_CLOUD
            Xamarin.Calabash.Start();
#endif

            Rg.Plugins.Popup.Popup.Init();

            Forms.SetFlags("CarouselView_Experimental", "IndicatorView_Experimental");
            Forms.Init();

            FFImageLoading.ImageService.Instance.Initialize();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();

            MessagingCenter.Subscribe<Core.App, string>(this, Constants.MessageShowToast, this.ShowToast);

            this.LoadApplication(new Core.App());

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        /// <summary>
        /// Called when a link with where-to-fly scheme is clicked.
        /// </summary>
        /// <param name="application">application object</param>
        /// <param name="url">url that was used</param>
        /// <param name="sourceApplication">source application; unused</param>
        /// <param name="annotation">annotation; unused</param>
        /// <returns>true always</returns>
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            var app = Core.App.Current as Core.App;
            if (url != null &&
                url.Scheme == Shared.Model.AppResourceUri.DefaultScheme)
            {
                Core.App.RunOnUiThread(async () => await app.OpenAppResourceUriAsync(url.ToString()));
            }

            return true;
        }

        /// <summary>
        /// Shows toast message with given text
        /// </summary>
        /// <param name="app">app object; unused</param>
        /// <param name="message">toast message</param>
        private void ShowToast(Core.App app, string message)
        {
            GlobalToast.Toast.MakeToast(message)
                .SetDuration(GlobalToast.ToastDuration.Long)
                .Show();
        }
    }
}
