using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Geo.DataFormats;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "Import locations" page
    /// </summary>
    public class ImportLocationsViewModel
    {
        /// <summary>
        /// Command to execute an import from included location list
        /// </summary>
        public Command ImportIncludedCommand { get; set; }

        /// <summary>
        /// Command to execute an import from a location list from storage
        /// </summary>
        public Command ImportFromStorageCommand { get; set; }

        /// <summary>
        /// Command to execute a download from web
        /// </summary>
        public Command DownloadFromWebCommand { get; set; }

        /// <summary>
        /// A mapping of display string to locations list filename, stored as Assets in the app
        /// </summary>
        private readonly Dictionary<string, string> includedLocationsList = new Dictionary<string, string>
        {
            { "Paraglidingspots European Alps", "paraglidingspots_european_alps_2018_01_07.kmz" },
            { "Paraglidingspots NZL", "paraglidingspots_nzl_2017_12_20.kmz" }
        };

        /// <summary>
        /// A mapping of display string to website address to open
        /// </summary>
        private readonly Dictionary<string, string> downloadWebSiteList = new Dictionary<string, string>
        {
            { "Paraglidingspots.com", "http://paraglidingspots.com/downloadselect.aspx" },
            { "DHV Gelände-Datenbank",
                "https://www.dhv.de/web/piloteninfos/gelaende-luftraum-natur/fluggelaendeflugbetrieb/gelaendedaten/gelaendedaten-download" }
        };

        /// <summary>
        /// Creates a new view model object
        /// </summary>
        public ImportLocationsViewModel()
        {
            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings objects
        /// </summary>
        private void SetupBindings()
        {
            this.ImportIncludedCommand = new Command(async () => await this.ImportIncludedAsync());
            this.ImportFromStorageCommand = new Command(async () => await this.ImportFromStorageAsync());
            this.DownloadFromWebCommand = new Command(async () => await this.DownloadFromWebAsync());
        }

        /// <summary>
        /// Presents a list of included location lists and imports it
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportIncludedAsync()
        {
            string result = await App.Current.MainPage.DisplayActionSheet(
                "Select a location list",
                "Cancel",
                null,
                this.includedLocationsList.Keys.ToArray());

            if (result == null ||
                !this.includedLocationsList.ContainsKey(result))
            {
                return;
            }

            string assetFilename = this.includedLocationsList[result];

            List<Location> locationList = await LoadLocationListFromAssetsAsync(assetFilename);

            if (locationList == null)
            {
                return;
            }

            bool appendToList = await AskAppendToList();

            var dataService = DependencyService.Get<IDataService>();

            if (appendToList)
            {
                var currentList = await dataService.GetLocationListAsync(CancellationToken.None);
                locationList.InsertRange(0, currentList);
            }

            await dataService.StoreLocationListAsync(locationList);

            App.UpdateMapLocationsList();

            await NavigationService.Instance.GoBack();

            App.ShowToast("Location list was loaded.");
        }

        /// <summary>
        /// Loads location list from assets
        /// </summary>
        /// <param name="assetFilename">asset base filename</param>
        /// <returns>list of locations, or null when no location list could be loaded</returns>
        private static async Task<List<Location>> LoadLocationListFromAssetsAsync(string assetFilename)
        {
            try
            {
                var platform = DependencyService.Get<IPlatform>();
                using (var stream = platform.OpenAssetStream("locations/" + assetFilename))
                {
                    return GeoLoader.LoadLocationList(stream, assetFilename);
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while loading location list: " + ex.Message,
                    "OK");

                return null;
            }
        }

        /// <summary>
        /// Opens a file requester and imports the selected file
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportFromStorageAsync()
        {
            FileData result = null;
            try
            {
                result = await CrossFilePicker.Current.PickFile();
                if (result == null ||
                    string.IsNullOrEmpty(result.FilePath))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while picking a file: " + ex.Message,
                    "OK");

                return;
            }

            List<Location> locationList = await LoadLocationListFromStorageAsync(result.FilePath);

            if (locationList == null)
            {
                return;
            }

            bool appendToList = await AskAppendToList();

            var dataService = DependencyService.Get<IDataService>();

            if (appendToList)
            {
                var currentList = await dataService.GetLocationListAsync(CancellationToken.None);
                locationList.InsertRange(0, currentList);
            }

            await dataService.StoreLocationListAsync(locationList);

            App.UpdateMapLocationsList();

            await NavigationService.Instance.GoBack();

            App.ShowToast("Location list was loaded.");
        }

        /// <summary>
        /// Loads location list from storage
        /// </summary>
        /// <param name="storageFilename">complete storage filename</param>
        /// <returns>list of locations, or null when no location list could be loaded</returns>
        private static async Task<List<Location>> LoadLocationListFromStorageAsync(string storageFilename)
        {
            try
            {
                return GeoLoader.LoadLocationList(storageFilename);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while loading location list: " + ex.Message,
                    "OK");

                return null;
            }
        }

        /// <summary>
        /// Presents a list of websites to download from and opens selected URL. Importing is then
        /// done using the file extension association.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task DownloadFromWebAsync()
        {
            string result = await App.Current.MainPage.DisplayActionSheet(
                "Select a web page to open",
                "Cancel",
                null,
                this.downloadWebSiteList.Keys.ToArray());

            if (result == null ||
                !this.downloadWebSiteList.ContainsKey(result))
            {
                return;
            }

            string webSiteToOpen = this.downloadWebSiteList[result];

            Device.OpenUri(new Uri(webSiteToOpen));
        }

        /// <summary>
        /// Asks user if location list should be appended or replaced
        /// </summary>
        /// <returns>true when location list should be appended, false when it should be
        /// replaced</returns>
        internal static async Task<bool> AskAppendToList()
        {
            bool appendToList = await App.Current.MainPage.DisplayAlert(
                Constants.AppTitle,
                "Append to current location list or replace list?",
                "Append",
                "Replace");

            return appendToList;
        }
    }
}
