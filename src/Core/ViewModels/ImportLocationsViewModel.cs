using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.Core.Services;
using WhereToFly.Logic;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.ViewModels
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
        /// Command to clear location list
        /// </summary>
        public Command ClearLocationsCommand { get; set; }

        /// <summary>
        /// A mapping of display string to locations list filename, stored as Assets in the app
        /// </summary>
        private readonly Dictionary<string, string> includedLocationsList = new Dictionary<string, string>
        {
            { "Alpen", "alpen.kmz" },
            { "Paraglidingspots NZL", "paraglidingspots_nzl_2017_12_20.kmz" }
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
            this.ClearLocationsCommand = new Command(async () => await this.ClearLocationsAsync());
        }

        /// <summary>
        /// Presents a list of included location lists and imports it
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportIncludedAsync()
        {
            string result = await App.Current.MainPage.DisplayActionSheet(
                Constants.AppTitle,
                "Cancel",
                null,
                this.includedLocationsList.Keys.ToArray());

            if (result == null)
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

            var dataService = DependencyService.Get<DataService>();

            if (appendToList)
            {
                var currentList = await dataService.GetLocationListAsync(CancellationToken.None);
                locationList.InsertRange(0, currentList);
            }

            await dataService.StoreLocationListAsync(locationList);

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
                    return GeoLoader.LoadLocationList(stream, isKml: false);
                }
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
        /// Opens a file requester and imports the selected file
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportFromStorageAsync()
        {
            // TODO selector
            string storageFilename = string.Empty;

            List<Location> locationList = await LoadLocationListFromStorageAsync(storageFilename);

            if (locationList == null)
            {
                return;
            }

            bool appendToList = await AskAppendToList();

            var dataService = DependencyService.Get<DataService>();

            if (appendToList)
            {
                var currentList = await dataService.GetLocationListAsync(CancellationToken.None);
                locationList.InsertRange(0, currentList);
            }

            await dataService.StoreLocationListAsync(locationList);

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
                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while loading location list: " + ex.Message,
                    "OK");

                return null;
            }
        }

        /// <summary>
        /// Clears all locations
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ClearLocationsAsync()
        {
            bool result = await App.Current.MainPage.DisplayAlert(
                Constants.AppTitle,
                "Really clear all locations?",
                "Clear",
                "Cancel");

            if (!result)
            {
                return;
            }

            var dataService = DependencyService.Get<DataService>();
            await dataService.StoreLocationListAsync(new List<Location>());

            await NavigationService.Instance.GoBack();

            App.ShowToast("Location list was cleared.");
        }

        /// <summary>
        /// Asks user if location list should be appended or replaced
        /// </summary>
        /// <returns>true when location list should be appended, false when it should be
        /// replaced</returns>
        private static async Task<bool> AskAppendToList()
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
