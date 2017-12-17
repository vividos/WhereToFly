using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using System;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.Core.Services;
using WhereToFly.Core.Views;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core
{
    /// <summary>
    /// Xamarin.Forms application for the WhereToFly app
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Application settings
        /// </summary>
        public static AppSettings Settings { get; internal set; }

        /// <summary>
        /// Creates a new app object
        /// </summary>
        public App()
        {
            AppCenter.Start(
                "android=" + Constants.AppCenterKeyAndroid,
                typeof(Distribute),
                typeof(Crashes));

            this.InitializeComponent();
            this.SetupDepencencyService();
            this.SetupMainPage();
            Task.Factory.StartNew(async () => await this.LoadAppSettingsAsync());
        }

        /// <summary>
        /// Sets up DependencyService
        /// </summary>
        private void SetupDepencencyService()
        {
            DependencyService.Register<NavigationService>();
            DependencyService.Register<DataService>();
        }

        /// <summary>
        /// Sets up MainPage
        /// </summary>
        private void SetupMainPage()
        {
            var navigationPage = new NavigationPage(new MapPage())
            {
                BarBackgroundColor = Constants.PrimaryColor
            };

            this.MainPage = navigationPage;
            NavigationService.Instance.NavigationPage = navigationPage;
        }

        /// <summary>
        /// Loads app settings and stores it in App.Settings static property
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadAppSettingsAsync()
        {
            var dataService = DependencyService.Get<DataService>();

            App.Settings = await dataService.GetAppSettingsAsync(CancellationToken.None);
        }

        /// <summary>
        /// Runs action on the UI thread
        /// </summary>
        /// <param name="action">action to run</param>
        public static void RunOnUiThread(Action action)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(action);
        }

        /// <summary>
        /// Runs action on the UI thread and waits for completion; async version
        /// </summary>
        /// <param name="action">action to run</param>
        /// <returns>task to wait on for completion</returns>
        public static Task RunOnUiThreadAsync(Action action)
        {
            var tcs = new TaskCompletionSource<object>();

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Shows toast message with given text
        /// </summary>
        /// <param name="message">toast message text</param>
        public static void ShowToast(string message)
        {
            var app = Xamarin.Forms.Application.Current as App;

            MessagingCenter.Send<App, string>(app, Constants.MessageShowToast, message);
        }

        #region App lifecycle methods
        /// <summary>
        /// Called when application is starting
        /// </summary>
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        /// <summary>
        /// Called when application is paused
        /// </summary>
        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        /// <summary>
        /// Called when application is resumed
        /// </summary>
        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        #endregion
    }
}
