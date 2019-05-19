using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Logic;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the location list page
    /// </summary>
    public class LocationListViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// App settings to store last used filter text
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        /// Location list
        /// </summary>
        private List<Location> locationList = new List<Location>();

        /// <summary>
        /// Backing store for FilterText property
        /// </summary>
        private string filterText;

        /// <summary>
        /// Timer that is triggered after filter text was updated
        /// </summary>
        private System.Timers.Timer filterTextUpdateTimer = new System.Timers.Timer(1000.0);

        /// <summary>
        /// Backing field for "IsListRefreshActive" property
        /// </summary>
        private bool isListRefreshActive;

        /// <summary>
        /// Current position of user; may be null when not retrieved yet
        /// </summary>
        private MapPoint currentPosition;

        /// <summary>
        /// A mapping of display string to locations list filename, stored as Assets in the app
        /// </summary>
        private readonly Dictionary<string, string> includedLocationsList = new Dictionary<string, string>
        {
            { "Paraglidingspots European Alps Mai 2019", "paraglidingspots_european_alps_2019_05_15.kmz" },
            { "Crossing the Alps 2018 Waypoints", "crossing_the_alps_2018.kmz" },
            { "Paraglidingspots Crossing the Alps 2019", "paraglidingspots_crossing_the_alps_2019_05_15.kmz" }
        };

        /// <summary>
        /// A mapping of display string to website address to open
        /// </summary>
        private readonly Dictionary<string, string> downloadWebSiteList = new Dictionary<string, string>
        {
            { "vividos' Where-to-fly resources", "http://www.vividos.de/wheretofly.html" },
            { "Paraglidingspots.com", "http://paraglidingspots.com/downloadselect.aspx" },
            { "DHV Gelände-Datenbank",
                "https://www.dhv.de/web/piloteninfos/gelaende-luftraum-natur/fluggelaendeflugbetrieb/gelaendedaten/gelaendedaten-download" }
        };

        #region Binding properties
        /// <summary>
        /// Current location list; may be filtered by filter text
        /// </summary>
        public ObservableCollection<LocationListEntryViewModel> LocationList { get; set; }

        /// <summary>
        /// Filter text string that filters entries by text
        /// </summary>
        public string FilterText
        {
            get
            {
                return this.filterText;
            }

            set
            {
                this.filterText = value;

                this.filterTextUpdateTimer.Stop();
                this.filterTextUpdateTimer.Start();
            }
        }

        /// <summary>
        /// Returns true when the location list has entries, but all entries were filtered out by
        /// the filter text
        /// </summary>
        public bool AreAllLocationsFilteredOut
        {
            get
            {
                return this.locationList != null &&
                    this.locationList.Any() &&
                    this.LocationList != null &&
                    !this.LocationList.Any();
            }
        }

        /// <summary>
        /// Indicates if the refreshing of the location list is currently active, in order to show
        /// an activity indicator.
        /// </summary>
        public bool IsListRefreshActive
        {
            get => this.isListRefreshActive;
            private set
            {
                this.isListRefreshActive = value;
                this.OnPropertyChanged(nameof(this.IsListRefreshActive));
            }
        }

        /// <summary>
        /// Indicates if the track list is empty.
        /// </summary>
        public bool IsListEmpty
        {
            get => !this.isListRefreshActive &&
                (this.locationList == null || !this.locationList.Any());
        }

        /// <summary>
        /// Command to execute when an item in the location list has been tapped
        /// </summary>
        public Command<Location> ItemTappedCommand { get; private set; }

        /// <summary>
        /// Command to execute when "import locations" context action is selected
        /// </summary>
        public Command ImportLocationsCommand { get; set; }

        /// <summary>
        /// Command to execute when "add tour plan location" conext action is selected
        /// </summary>
        public Command AddTourPlanLocationCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model object for location list
        /// </summary>
        /// <param name="appSettings">app settings to use</param>
        public LocationListViewModel(AppSettings appSettings)
        {
            this.appSettings = appSettings;

            this.filterText = appSettings.LastLocationListFilterText;

            this.filterTextUpdateTimer.Elapsed += async (sender, args) =>
            {
                this.filterTextUpdateTimer.Stop();

                await StoreLastLocationListFilterText(this.FilterText);

                this.UpdateLocationList();
            };

            this.isListRefreshActive = false;

            this.SetupBindings();
        }

        /// <summary>
        /// Stores last location list filter text
        /// </summary>
        /// <param name="filterText">filter text to store</param>
        /// <returns>task to wait on</returns>
        private async Task StoreLastLocationListFilterText(string filterText)
        {
            this.appSettings.LastLocationListFilterText = filterText;

            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(this.appSettings);
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            Task.Run(this.LoadDataAsync);

            this.ItemTappedCommand =
                new Command<Location>(async (location) =>
                {
                    await this.NavigateToLocationDetails(location);
                });

            this.ImportLocationsCommand =
                new Command(async () => await this.ImportLocationsAsync());

            this.AddTourPlanLocationCommand =
                new Command<Location>((location) => App.AddTourPlanLocation(location));
        }

        /// <summary>
        /// Loads data; async method
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadDataAsync()
        {
            try
            {
                IDataService dataService = DependencyService.Get<IDataService>();

                this.locationList = await dataService.GetLocationListAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }

            this.UpdateLocationList();
        }

        /// <summary>
        /// Updates location list based on filter and current position
        /// </summary>
        private void UpdateLocationList()
        {
            Task.Run(() =>
            {
                this.IsListRefreshActive = true;

                var newList = this.locationList
                    .Select(location => new LocationListEntryViewModel(this, location, this.currentPosition));

                if (!string.IsNullOrWhiteSpace(this.filterText))
                {
                    newList = newList.Where(viewModel => this.IsFilterMatch(viewModel));
                }

                if (this.currentPosition != null)
                {
                    newList = newList.OrderBy(viewModel => viewModel.Distance);
                }

                this.LocationList = new ObservableCollection<LocationListEntryViewModel>(newList);

                this.OnPropertyChanged(nameof(this.LocationList));
                this.OnPropertyChanged(nameof(this.AreAllLocationsFilteredOut));
                this.OnPropertyChanged(nameof(this.IsListEmpty));

                this.IsListRefreshActive = false;
            });
        }

        /// <summary>
        /// Checks if reload is needed, e.g. when location list has changed.
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task CheckReloadNeeded()
        {
            try
            {
                IDataService dataService = DependencyService.Get<IDataService>();

                var newLocationList = await dataService.GetLocationListAsync(CancellationToken.None);

                if (this.locationList.Count != newLocationList.Count ||
                    !Enumerable.SequenceEqual(this.locationList, newLocationList, new LocationEqualityComparer()))
                {
                    this.locationList = newLocationList;

                    this.UpdateLocationList();
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }
        }

        /// <summary>
        /// Checks if given location (represented by a view model) is a current filter match,
        /// based on the filter text.
        /// </summary>
        /// <param name="viewModel">location view model to check</param>
        /// <returns>matching filter</returns>
        private bool IsFilterMatch(LocationListEntryViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(this.filterText))
            {
                return true;
            }

            string text = this.filterText;
            var location = viewModel.Location;

            bool inName = location.Name != null &&
                location.Name.IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;

            bool inDescription = !inName &&
                location.Description != null &&
                location.Description.IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;

            bool inInternetLink = !inDescription &&
                location.InternetLink != null &&
                location.InternetLink.IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;

            bool inMapLocation = !inInternetLink &&
                location.MapLocation != null &&
                location.MapLocation.ToString().IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;

            bool inDistance = !inMapLocation &&
                Math.Abs(viewModel.Distance) > 1e-6 &&
                DataFormatter.FormatDistance(viewModel.Distance).IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;

            return
                inName ||
                inDescription ||
                inInternetLink ||
                inMapLocation ||
                inDistance;
        }

        /// <summary>
        /// Navigates to location info page, showing details about given location
        /// </summary>
        /// <param name="location">location to show</param>
        /// <returns>task to wait on</returns>
        internal async Task NavigateToLocationDetails(Location location)
        {
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyLocationDetailsPage, true, location);
        }

        /// <summary>
        /// Returns to map view and zooms to the given location
        /// </summary>
        /// <param name="location">location to zoom to</param>
        /// <returns>task to wait on</returns>
        internal async Task ZoomToLocation(Location location)
        {
            App.ZoomToLocation(location.MapLocation);

            App.UpdateMapLocationsList();
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyMapPage, animated: true);
        }

        /// <summary>
        /// Shows menu to import location lists
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportLocationsAsync()
        {
            var importActions = new List<string>
            {
                "Import included",
                "Import from storage",
                "Download from web"
            };

            string result = await App.Current.MainPage.DisplayActionSheet(
                $"Import location",
                "Cancel",
                null,
                importActions.ToArray());

            if (!string.IsNullOrEmpty(result))
            {
                int selectedIndex = importActions.IndexOf(result);

                switch (selectedIndex)
                {
                    case 0:
                        await this.ImportIncludedAsync();
                        break;

                    case 1:
                        await this.ImportFromStorageAsync();
                        break;

                    case 2:
                        await this.DownloadFromWebAsync();
                        break;

                    default:
                        // ignore
                        break;
                }
            }
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

            await this.ReloadLocationListAsync();
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
            FileData result;
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

            await this.ReloadLocationListAsync();
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
        /// Deletes the given location from the location list
        /// </summary>
        /// <param name="location">location to delete</param>
        /// <returns>task to wait on</returns>
        internal async Task DeleteLocation(Location location)
        {
            this.locationList.Remove(location);

            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreLocationListAsync(this.locationList);

            var liveWaypointRefreshService = DependencyService.Get<LiveWaypointRefreshService>();
            liveWaypointRefreshService.UpdateLiveWaypointList(locationList);

            this.UpdateLocationList();

            App.UpdateMapLocationsList();

            App.ShowToast("Selected location was deleted.");
        }

        /// <summary>
        /// Clears all locations
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ClearLocationsAsync()
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

            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreLocationListAsync(new List<Location>());

            var liveWaypointRefreshService = DependencyService.Get<LiveWaypointRefreshService>();
            liveWaypointRefreshService.ClearLiveWaypointList();

            await this.ReloadLocationListAsync();

            App.UpdateMapLocationsList();

            App.ShowToast("Location list was cleared.");
        }

        /// <summary>
        /// Reloads location list and shows it on the page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ReloadLocationListAsync()
        {
            await this.LoadDataAsync();
            this.UpdateLocationList();
        }

        /// <summary>
        /// Called when position has changed; updates distance of locations to the current
        /// position and updates location list
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args, including position</param>
        public void OnPositionChanged(object sender, PositionEventArgs args)
        {
            this.currentPosition = new MapPoint(args.Position.Latitude, args.Position.Longitude, args.Position.Altitude);

            this.UpdateLocationList();
        }

        #region IDisposable Support
        /// <summary>
        /// To detect redundant calls
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Disposes of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing">
        /// true when called from Dispose(), false when called from finalizer
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.filterTextUpdateTimer.Stop();
                    this.filterTextUpdateTimer.Dispose();
                    this.filterTextUpdateTimer = null;
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
