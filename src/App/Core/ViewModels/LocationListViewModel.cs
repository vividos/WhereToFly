using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Views;
using WhereToFly.Geo.Model;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the location list page
    /// </summary>
    public class LocationListViewModel : ViewModelBase
    {
        /// <summary>
        /// A mapping of display string to locations list filename, stored as Assets in the app
        /// </summary>
        private readonly Dictionary<string, string> includedLocationsList = new()
        {
            { "Example Schliersee locations", "default" },
            { "Paraglidingspots European Alps Vs. 2.02", "paraglidingspots_european_alps_2022_03_29.kmz" },
        };

        /// <summary>
        /// A mapping of display string to website address to open
        /// </summary>
        private readonly Dictionary<string, string> downloadWebSiteList = new()
        {
            { "vividos' Where-to-fly resources", "http://www.vividos.de/wheretofly/index.html" },
            { "Paraglidingspots.com", "https://paraglidingspots.com/downloadselect.aspx" },
            {
                "DHV Gelände-Datenbank",
                "https://www.dhv.de/web/piloteninfos/gelaende-luftraum-natur/fluggelaendeflugbetrieb/gelaendedaten/gelaendedaten-download"
            },
            { "Tourenwelt.info Jo's Hüttenliste", "http://www.tourenwelt.info/huettenliste/map.php" },
        };

        /// <summary>
        /// App settings to store last used filter text
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        /// Location list
        /// </summary>
        private List<Location> locationList = new();

        /// <summary>
        /// Backing field for "IsListRefreshActive" property
        /// </summary>
        private bool isListRefreshActive;

        /// <summary>
        /// Backing field for "IsListEmpty" property
        /// </summary>
        private bool isListEmpty = true;

        /// <summary>
        /// Current position of user; may be null when not retrieved yet
        /// </summary>
        private MapPoint currentPosition;

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
            get => this.appSettings.LastLocationFilterSettings.FilterText;
            set => this.appSettings.LastLocationFilterSettings.FilterText = value;
        }

        /// <summary>
        /// Command to execute when find text has finished entering
        /// </summary>
        public ICommand FindTextEnteredCommand { get; private set; }

        /// <summary>
        /// Takeoff directions filter value
        /// </summary>
        public TakeoffDirections FilterTakeoffDirections
        {
            get => this.appSettings.LastLocationFilterSettings.FilterTakeoffDirections;
            set => this.appSettings.LastLocationFilterSettings.FilterTakeoffDirections = value;
        }

        /// <summary>
        /// Command to execute when user taps on the "filter takeoff directions" view
        /// </summary>
        public ICommand FilterTakeoffDirectionsCommand { get; private set; }

        /// <summary>
        /// Returns true when the location list has entries, but all entries were filtered out by
        /// the filter text
        /// </summary>
        public bool AreAllLocationsFilteredOut =>
            !this.IsListRefreshActive &&
            !this.IsListEmpty &&
            this.LocationList != null &&
            !this.LocationList.Any();

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
                this.OnPropertyChanged(nameof(this.IsListEmpty));
                this.OnPropertyChanged(nameof(this.AreAllLocationsFilteredOut));
            }
        }

        /// <summary>
        /// Indicates if the track list is empty.
        /// </summary>
        public bool IsListEmpty =>
            !this.IsListRefreshActive && this.isListEmpty;

        /// <summary>
        /// Stores the selected location when an item is tapped
        /// </summary>
        public LocationListEntryViewModel SelectedLocation { get; set; }

        /// <summary>
        /// Command to execute when "import locations" toolbar item is selected
        /// </summary>
        public ICommand ImportLocationsCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model object for location list
        /// </summary>
        /// <param name="appSettings">app settings to use</param>
        public LocationListViewModel(AppSettings appSettings)
        {
            this.appSettings = appSettings;

            this.isListRefreshActive = false;

            this.SetupBindings();
        }

        /// <summary>
        /// Stores last location filter settings
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task StoreLastLocationFilterSettings()
        {
            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(this.appSettings);
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            this.UpdateLocationList();

            this.FindTextEnteredCommand =
                new AsyncCommand(this.OnFindTextEntered);

            this.FilterTakeoffDirectionsCommand =
                new AsyncCommand(this.FilterTakeoffDirectionsAsync);

            this.ImportLocationsCommand = new AsyncCommand(this.ImportLocationsAsync);
        }

        /// <summary>
        /// Called when find text was finished entering
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnFindTextEntered()
        {
            await this.StoreLastLocationFilterSettings();

            this.UpdateLocationList();
        }

        /// <summary>
        /// Updates location list based on filter and current position
        /// </summary>
        public void UpdateLocationList()
        {
            Task.Run(async () =>
            {
                this.IsListRefreshActive = true;

                IDataService dataService = DependencyService.Get<IDataService>();
                var locationDataService = dataService.GetLocationDataService();

                this.isListEmpty = await locationDataService.IsListEmpty();

                IEnumerable<Location> localLocationList = await locationDataService.GetList(
                    this.appSettings.LastLocationFilterSettings);

                this.locationList = localLocationList.ToList();

                var newList = this.locationList
                    .Select(location => new LocationListEntryViewModel(this, location, this.currentPosition));

                if (this.currentPosition != null)
                {
                    newList = newList.OrderBy(viewModel => viewModel.Distance);
                }

                this.LocationList = new ObservableCollection<LocationListEntryViewModel>(newList);

                this.OnPropertyChanged(nameof(this.LocationList));
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
                var locationDataService = dataService.GetLocationDataService();

                var newLocationList = await locationDataService.GetList(
                    this.appSettings.LastLocationFilterSettings);

                if (!Enumerable.SequenceEqual(this.locationList, newLocationList))
                {
                    this.isListEmpty = await locationDataService.IsListEmpty();
                    this.locationList = newLocationList.ToList();

                    this.UpdateLocationList();
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }
        }

        /// <summary>
        /// Shows the "filter takeoff directions popup page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task FilterTakeoffDirectionsAsync()
        {
            var result = await FilterTakeoffDirectionsPopupPage.ShowAsync(
                this.appSettings.LastLocationFilterSettings);

            if (result != null)
            {
                this.appSettings.LastLocationFilterSettings = result;
                this.OnPropertyChanged(nameof(this.FilterTakeoffDirections));

                await this.StoreLastLocationFilterSettings();

                this.UpdateLocationList();
            }
        }

        /// <summary>
        /// Navigates to location info page, showing details about given location
        /// </summary>
        /// <param name="location">location to show</param>
        /// <returns>task to wait on</returns>
        internal async Task NavigateToLocationDetails(Location location)
        {
            await NavigationService.Instance.NavigateAsync(PageKey.LocationDetailsPage, true, location);
        }

        /// <summary>
        /// Returns to map view and zooms to the given location
        /// </summary>
        /// <param name="location">location to zoom to</param>
        /// <returns>task to wait on</returns>
        internal async Task ZoomToLocation(Location location)
        {
            await App.UpdateLastShownPositionAsync(location.MapLocation);

            App.MapView.ZoomToLocation(location.MapLocation);

            await NavigationService.GoToMap();
        }

        /// <summary>
        /// Returns to map view and sets the compass target
        /// </summary>
        /// <param name="location">location to use as target</param>
        /// <returns>task to wait on</returns>
        internal async Task SetAsCompassTarget(Location location)
        {
            var compassTarget = new CompassTarget
            {
                Title = location.Name,
                TargetLocation = location.MapLocation,
            };

            await App.SetCompassTarget(compassTarget);

            await NavigationService.GoToMap();
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
                "Download from web",
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

            if (assetFilename == "default")
            {
                await OpenFileHelper.AddDefaultLocationListAsync();
            }
            else
            {
                var platform = DependencyService.Get<IPlatform>();
                using var stream = platform.OpenAssetStream("locations/" + assetFilename);
                await OpenFileHelper.OpenLocationListAsync(stream, assetFilename);
            }

            this.UpdateLocationList();
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
            try
            {
                var options = new Xamarin.Essentials.PickOptions
                {
                    FileTypes = new Xamarin.Essentials.FilePickerFileType(
                        new Dictionary<Xamarin.Essentials.DevicePlatform, IEnumerable<string>>
                        {
                            { Xamarin.Essentials.DevicePlatform.Android, null },
                            { Xamarin.Essentials.DevicePlatform.UWP, new string[] { ".kml", ".kmz", ".gpx", ".cup" } },
                            { Xamarin.Essentials.DevicePlatform.iOS, null },
                        }),
                    PickerTitle = "Select a Location file to import",
                };

                var result = await Xamarin.Essentials.FilePicker.PickAsync(options);
                if (result == null ||
                    string.IsNullOrEmpty(result.FullPath))
                {
                    return;
                }

                using var stream = await result.OpenReadAsync();
                await OpenFileHelper.OpenLocationListAsync(stream, result.FileName);
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

            this.UpdateLocationList();
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

            await Xamarin.Essentials.Browser.OpenAsync(
                webSiteToOpen,
                Xamarin.Essentials.BrowserLaunchMode.External);
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
            var locationDataService = dataService.GetLocationDataService();

            await locationDataService.Remove(location.Id);

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.RemoveLiveWaypoint(location.Id);

            this.UpdateLocationList();

            App.MapView.RemoveLocation(location.Id);

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
            var locationDataService = dataService.GetLocationDataService();

            await locationDataService.ClearList();

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.ClearLiveWaypointList();

            this.UpdateLocationList();

            App.MapView.ClearLocationList();

            App.ShowToast("Location list was cleared.");
        }

        /// <summary>
        /// Called when position has changed; updates distance of locations to the current
        /// position and updates location list
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args, including position</param>
        public void OnPositionChanged(object sender, GeolocationEventArgs args)
        {
            this.currentPosition = args.Point;

            this.UpdateLocationList();
        }
    }
}
