using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.Core.Services;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.ViewModels
{
    /// <summary>
    /// View model for the location list page
    /// </summary>
    public class LocationListViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Location list
        /// </summary>
        private List<Location> locationList = new List<Location>();

        /// <summary>
        /// Backing store for FilterText property
        /// </summary>
        private string filterText = string.Empty;

        #region Binding properties
        /// <summary>
        /// Current location list; may be filtered by filter text
        /// </summary>
        public ObservableCollection<LocationInfoViewModel> LocationList { get; set; }

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
                this.UpdateLocationList();
            }
        }
        #endregion

        /// <summary>
        /// Creates a new view model object for location list
        /// </summary>
        public LocationListViewModel()
        {
            Task.Factory.StartNew(this.LoadDataAsync);
        }

        /// <summary>
        /// Loads data; async method
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadDataAsync()
        {
            var dataService = DependencyService.Get<DataService>();

            this.locationList = await dataService.GetLocationListAsync(CancellationToken.None);

            this.UpdateLocationList();
        }

        /// <summary>
        /// Updates location list based on filter
        /// </summary>
        private void UpdateLocationList()
        {
            if (string.IsNullOrWhiteSpace(this.filterText))
            {
                var locationViewModelList =
                    from location in this.locationList
                    select new LocationInfoViewModel(location);

                this.LocationList = new ObservableCollection<LocationInfoViewModel>(locationViewModelList);
            }
            else
            {
                var filteredLocationList =
                    from location in this.locationList
                    where this.IsFilterMatch(location)
                    select new LocationInfoViewModel(location);

                this.LocationList = new ObservableCollection<LocationInfoViewModel>(filteredLocationList);
            }

            this.OnPropertyChanged(nameof(this.LocationList));
        }

        /// <summary>
        /// Checks if given location is a current filter match, based on the filter text
        /// </summary>
        /// <param name="location">location to check</param>
        /// <returns>matching filter</returns>
        private bool IsFilterMatch(Location location)
        {
            if (string.IsNullOrWhiteSpace(this.filterText))
            {
                return true;
            }

            string text = this.filterText;

            bool inName = location.Name != null && location.Name.IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;
            bool inDescription = location.Description != null && location.Description.IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;
            bool inInternetLink = location.InternetLink != null && location.InternetLink.IndexOf(text, 0, StringComparison.OrdinalIgnoreCase) >= 0;
            bool inMapLocation = location.MapLocation != null && text.IndexOf(location.MapLocation.ToString(), 0, StringComparison.OrdinalIgnoreCase) >= 0;
            bool inElevation = location.Elevation != 0 && text.IndexOf(((int)location.Elevation).ToString(), 0, StringComparison.OrdinalIgnoreCase) >= 0;

            return
                inName ||
                inDescription ||
                inInternetLink ||
                inMapLocation ||
                inElevation;
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
    }
}
