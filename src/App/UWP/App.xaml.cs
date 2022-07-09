using System;
using System.IO;
using System.Net.Http;
using WhereToFly.App.Core;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WhereToFly.App.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.SetupRootFrame(args, args.Arguments);
        }

        /// <summary>
        /// Sets up root frame, if not already existing
        /// </summary>
        /// <param name="args">event args for activation event</param>
        /// <param name="parameters">page parameters</param>
        private void SetupRootFrame(IActivatedEventArgs args, object parameters)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += this.OnNavigationFailed;

                this.SetupApp(args);

                //// Note: When needed, use this check and restore state
                //// if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), parameters);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Sets up Xamarin app and all plugins
        /// </summary>
        /// <param name="args">event args for activation event</param>
        private void SetupApp(IActivatedEventArgs args)
        {
            Rg.Plugins.Popup.Popup.Init();

            Xamarin.Forms.Forms.Init(args, Rg.Plugins.Popup.Popup.GetExtraAssemblies());

            var imageLoadingConfig = new FFImageLoading.Config.Configuration
            {
                HttpClient = new HttpClient(new FFImageLoadingHttpClientHandler()),
            };

            FFImageLoading.ImageService.Instance.Initialize(imageLoadingConfig);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();

            Xamarin.Essentials.Platform.MapServiceToken = Constants.BingMapsKeyUwp;

            Xamarin.Forms.MessagingCenter.Subscribe<Core.App, string>(this, Constants.MessageShowToast, this.ShowToast);
        }

        /// <summary>
        /// Called when the UWP app is activated, e.g. by a Protocol link.
        /// </summary>
        /// <param name="args">event args</param>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                var eventArgs = args as ProtocolActivatedEventArgs;

                Core.App.OpenAppResourceUri(eventArgs.Uri.AbsoluteUri);
            }
        }

        /// <summary>
        /// Called when a file associated with the UWP app is opened.
        /// </summary>
        /// <param name="args">file activation event args</param>
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            this.SetupRootFrame(args, null);

            if (args.Files.Count > 0)
            {
                var file = args.Files[0] as StorageFile;

                Core.App.RunOnUiThread(async () =>
                {
                    using var stream = await file.OpenStreamForReadAsync();
                    await OpenFileHelper.OpenFileAsync(stream, file.Name);
                });
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new InvalidOperationException("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // Note: When needed, save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Shows toast message with given text
        /// </summary>
        /// <param name="app">sender app object</param>
        /// <param name="message">toast message</param>
        private void ShowToast(Core.App app, string message)
        {
            ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();

            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(Constants.AppTitle));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(message));

            var toast = new ToastNotification(toastXml)
            {
                ExpirationTime = DateTime.Now.AddSeconds(4),
            };
            toastNotifier.Show(toast);
        }
    }
}
