using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Logic;
using Windows.Storage;

namespace WhereToFly.App.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Creates a new MAUI app
        /// </summary>
        /// <returns>MAUI app</returns>
        protected override MauiApp CreateMauiApp()
            => MauiProgram.CreateMauiApp();

        /// <summary>
        /// Called when the app is launched or activated
        /// </summary>
        /// <param name="args">event args</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var activatedEventArgs = AppInstance.GetCurrent()?.GetActivatedEventArgs();
            if (activatedEventArgs != null &&
                activatedEventArgs.Kind != ExtendedActivationKind.Launch)
            {
                OnActivated(activatedEventArgs);
            }

            base.OnLaunched(args);
        }

        /// <summary>
        /// Called when the WinUI app is activated, e.g. by a Protocol link.
        /// </summary>
        /// <param name="args">event args</param>
        private static void OnActivated(AppActivationArguments args)
        {
            if (args.Kind == ExtendedActivationKind.Protocol &&
                args.Data is Windows.ApplicationModel.Activation.ProtocolActivatedEventArgs protocolActivatedEventArgs &&
                protocolActivatedEventArgs.Uri?.AbsoluteUri != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var appMapService = WhereToFly.App.App.Services.GetRequiredService<IAppMapService>();
                    appMapService.OpenAppResourceUri(protocolActivatedEventArgs.Uri.AbsoluteUri);
                });
            }

            if (args.Kind == ExtendedActivationKind.File &&
                args.Data is Windows.ApplicationModel.Activation.FileActivatedEventArgs fileActivatedEventArgs &&
                fileActivatedEventArgs.Files.Count > 0 &&
                fileActivatedEventArgs.Files[0] is StorageFile file &&
                !string.IsNullOrEmpty(file.Name))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    using var stream = await file.OpenStreamForReadAsync();
                    await OpenFileHelper.OpenFileAsync(stream, file.Name);
                });
            }
        }
    }
}
