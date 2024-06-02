using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
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
        /// Configures all lifecycle events for the Windows app
        /// </summary>
        /// <param name="lifecycleBuilder">lifecycle builder</param>
        public static void AddLifecycleEvents(ILifecycleBuilder lifecycleBuilder)
        {
            lifecycleBuilder.AddWindows(
                windows => windows
                .OnWindowCreated(
                    window =>
                    {
                        var nativeWindow = window as MauiWinUIWindow;
                        var titleBar = nativeWindow?.AppWindow?.TitleBar;

                        if (titleBar != null)
                        {
                            titleBar.BackgroundColor =
                            titleBar.InactiveBackgroundColor =
                            titleBar.ButtonBackgroundColor =
                                Windows.UI.Color.FromArgb(0xFF, 0x2F, 0x29, 0x9E);
                        }
                    }));
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
                    var appMapService = DependencyService.Get<IAppMapService>();
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
