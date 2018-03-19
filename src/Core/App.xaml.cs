using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.Core.Services;
using WhereToFly.Core.Views;
using WhereToFly.Geo.DataFormats;
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
        /// Sets up MainPage; it contains a MapPage instance.
        /// </summary>
        private void SetupMainPage()
        {
            var mapPage = new MapPage();
            var navigationPage = new NavigationPage(mapPage)
            {
                BarBackgroundColor = Constants.PrimaryColor
            };

            NavigationPage.SetTitleIcon(mapPage, "icon.png");

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
        /// Opens and load location list from given stream object
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="filename">
        /// filename; extension of filename is used to determine file type
        /// </param>
        /// <returns>task to wait on</returns>
        public async Task OpenLocationListAsync(Stream stream, string filename)
        {
            List<Location> locationList = await LoadLocationListFromStreamAsync(stream, filename);

            if (locationList == null)
            {
                return;
            }

            bool appendToList = await ViewModels.ImportLocationsViewModel.AskAppendToList();

            var dataService = DependencyService.Get<DataService>();

            if (appendToList)
            {
                var currentList = await dataService.GetLocationListAsync(CancellationToken.None);
                locationList.InsertRange(0, currentList);
            }

            await dataService.StoreLocationListAsync(locationList);

            if (NavigationService.Instance.NavigationPage.CurrentPage is MapPage mapPage)
            {
                await mapPage.ReloadLocationListAsync();
            }

            App.ShowToast("Location list was loaded.");
        }

        /// <summary>
        /// Loads location list from assets
        /// </summary>
        /// <param name="stream">stream to load from</param>
        /// <param name="filename">
        /// filename; extension of filename is used to determine file type
        /// </param>
        /// <returns>list of locations, or null when no location list could be loaded</returns>
        private static async Task<List<Location>> LoadLocationListFromStreamAsync(Stream stream, string filename)
        {
            try
            {
                return GeoLoader.LoadLocationList(stream, filename);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while loading location list: " + ex.Message,
                    "OK");

                return null;
            }
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
