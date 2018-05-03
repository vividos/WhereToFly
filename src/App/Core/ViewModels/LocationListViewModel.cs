using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Geo.Spatial;
using WhereToFly.App.Logic;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the location list page
    /// </summary>
    public class LocationListViewModel : INotifyPropertyChanged, IDisposable
    {
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
        /// Current position of user; may be null when not retrieved yet
        /// </summary>
        private LatLongAlt currentPosition;

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
        /// Command to execute when an item in the location list has been tapped
        /// </summary>
        public Command<Location> ItemTappedCommand { get; private set; }
        #endregion

        /// <summary>
        /// Creates a new view model object for location list
        /// </summary>
        /// <param name="appSettings">app settings to use</param>
        public LocationListViewModel(AppSettings appSettings)
        {
            this.filterText = appSettings.LastLocationListFilterText;

            this.filterTextUpdateTimer.Elapsed += (sender, args) =>
            {
                this.filterTextUpdateTimer.Stop();
                App.RunOnUiThread(() => this.UpdateLocationList());
            };

            this.SetupBindings();
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

            bool inElevation = !inMapLocation &&
                location.Elevation != 0 &&
                ((int)location.Elevation).ToString().IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;

            bool inDistance = !inElevation &&
                Math.Abs(viewModel.Distance) > 1e-6 &&
                DataFormatter.FormatDistance(viewModel.Distance).IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;

            return
                inName ||
                inDescription ||
                inInternetLink ||
                inMapLocation ||
                inElevation ||
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
            await NavigationService.Instance.GoBack();
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

            this.UpdateLocationList();

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

            await this.ReloadLocationListAsync();

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
            this.currentPosition = new LatLongAlt(args.Position.Latitude, args.Position.Longitude);

            this.UpdateLocationList();
        }

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Event that gets signaled when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Call this method to signal that a property has changed
        /// </summary>
        /// <param name="propertyName">property name; use C# 6 nameof() operator</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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
