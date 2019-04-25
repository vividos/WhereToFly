using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            { "Crossing the Alps 2018 Waypoints", "crossing_the_alps_2018.kmz" },
            { "Paraglidingspots Crossing the Alps 2018", "paraglidingspots_crossing_the_alps_2018_06_19.kmz" }
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
            string assetFilename = await this.AskIncludedLocationListAsync();
            if (assetFilename == null)
            {
                return;
            }

            var platform = DependencyService.Get<IPlatform>();
            using (var stream = platform.OpenAssetStream("locations/" + assetFilename))
            {
                await OpenFileHelper.OpenLocationListAsync(stream, assetFilename);
            }
        }

        /// <summary>
        /// Asks user for included location list and returns the asset filename.
        /// </summary>
        /// <returns>asset filename, or null when no location list was selected</returns>
        private async Task<string> AskIncludedLocationListAsync()
        {
            string result = await App.Current.MainPage.DisplayActionSheet(
                "Select a location list",
                "Cancel",
                null,
                this.includedLocationsList.Keys.ToArray());

            if (result == null ||
                !this.includedLocationsList.ContainsKey(result))
            {
                return null;
            }

            return this.includedLocationsList[result];
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
                string[] fileTypes = null;

                if (Device.RuntimePlatform == Device.UWP)
                {
                    fileTypes = new string[] { ".kml", ".kmz", ".gpx" };
                }

                result = await CrossFilePicker.Current.PickFile(fileTypes);
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

            using (var stream = result.GetStream())
            {
                await OpenFileHelper.OpenLocationListAsync(stream, result.FileName);
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
    }
}
