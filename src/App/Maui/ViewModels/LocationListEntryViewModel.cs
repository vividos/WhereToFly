﻿using System.Windows.Input;
using WhereToFly.App.Logic;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for a single location list entry
    /// </summary>
    public class LocationListEntryViewModel
    {
        /// <summary>
        /// Parent view model
        /// </summary>
        private readonly LocationListViewModel parentViewModel;

        /// <summary>
        /// Location to show
        /// </summary>
        private readonly Location location;

        /// <summary>
        /// Property containing the location object
        /// </summary>
        public Location Location => this.location;

        /// <summary>
        /// Property containing location name
        /// </summary>
        public string Name
        {
            get
            {
                return this.location.Name;
            }
        }

        /// <summary>
        /// Returns image source for SvgImage in order to display the type image
        /// </summary>
        public ImageSource? TypeImageSource { get; }

        /// <summary>
        /// Returns if takeoff directions view should be visible at all
        /// </summary>
        public bool IsTakeoffDirectionsVisible
            => this.location.Type == LocationType.FlyingTakeoff;

        /// <summary>
        /// Takeoff directions flags; only set for FlyingTakeoff locations
        /// </summary>
        public TakeoffDirections TakeoffDirections
            => this.location.TakeoffDirections;

        /// <summary>
        /// Property containing detail infos for location
        /// </summary>
        public string DetailInfos
        {
            get
            {
                double altitude = this.location.MapLocation.Altitude.GetValueOrDefault(0.0);

                bool isAltitudeAvailable = Math.Abs(altitude) > 1e-6;

                return string.Format(
                    isAltitudeAvailable ? "Elevation: {0:F1} m; Distance: {1}" : "Distance: {1}",
                    altitude,
                    DataFormatter.FormatDistance(this.Distance));
            }
        }

        /// <summary>
        /// Property containing location description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Distance to the user's current location
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Command to execute when the item has been tapped
        /// </summary>
        public ICommand ItemTappedCommand { get; private set; }

        /// <summary>
        /// Command to execute when "show details" context menu item is selected on a location
        /// </summary>
        public ICommand ShowDetailsLocationCommand => this.ItemTappedCommand;

        /// <summary>
        /// Command to execute when "zoom to" context menu item is selected on a location
        /// </summary>
        public ICommand ZoomToLocationCommand { get; set; }

        /// <summary>
        /// Command to execute when "set as compass target" context menu item is selected on a location
        /// </summary>
        public ICommand SetAsCompassTargetCommand { get; set; }

        /// <summary>
        /// Command to execute when "delete" context menu item is selected on a location
        /// </summary>
        public ICommand DeleteLocationCommand { get; set; }

        /// <summary>
        /// Command to execute when "add tour plan location" toolbar item item is selected
        /// </summary>
        public ICommand AddTourPlanLocationCommand { get; set; }

        /// <summary>
        /// Creates a new view model object based on the given location object
        /// </summary>
        /// <param name="parentViewModel">parent view model</param>
        /// <param name="location">location object</param>
        /// <param name="myCurrentPosition">the user's current position; may be null</param>
        public LocationListEntryViewModel(
            LocationListViewModel parentViewModel,
            Location location,
            MapPoint? myCurrentPosition)
        {
            this.parentViewModel = parentViewModel;
            this.location = location;

            this.Distance = myCurrentPosition != null
                ? myCurrentPosition.DistanceTo(this.location.MapLocation)
                : 0.0;

            this.TypeImageSource =
                SvgImageCache.GetImageSource(this.location);

            this.Description = HtmlConverter.StripAllTags(this.location.Description);

            var appMapService = DependencyService.Get<IAppMapService>();

            this.ItemTappedCommand = new AsyncRelayCommand(this.OnShowDetailsLocation);
            this.ZoomToLocationCommand = new AsyncRelayCommand(this.OnZoomToLocationAsync);
            this.SetAsCompassTargetCommand = new AsyncRelayCommand(this.OnSetAsCompassTargetAsync);
            this.DeleteLocationCommand = new AsyncRelayCommand(this.OnDeleteLocationAsync);
            this.AddTourPlanLocationCommand =
                new AsyncRelayCommand(
                    () => appMapService.AddTourPlanLocation(this.location));
        }

        /// <summary>
        /// Called when "show details" context menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnShowDetailsLocation()
        {
            await this.parentViewModel.NavigateToLocationDetails(this.location);
        }

        /// <summary>
        /// Called when "zoom to" context menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToLocationAsync()
        {
            await this.parentViewModel.ZoomToLocation(this.location);
        }

        /// <summary>
        /// Called when "set as compass target" context menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnSetAsCompassTargetAsync()
        {
            await this.parentViewModel.SetAsCompassTarget(this.location);
        }

        /// <summary>
        /// Called when "delete" context menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteLocationAsync()
        {
            await this.parentViewModel.DeleteLocation(this.location);
        }
    }
}
