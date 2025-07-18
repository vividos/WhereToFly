﻿using System.Windows.Input;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;
using WhereToFly.App.Resources;
using WhereToFly.App.Services;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using Location = WhereToFly.Geo.Model.Location;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the location details page
    /// </summary>
    public class LocationDetailsViewModel : ViewModelBase
    {
        /// <summary>
        /// App settings object
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        /// Location to show
        /// </summary>
        private readonly Location location;

        /// <summary>
        /// Distance to the user's current location
        /// </summary>
        private double distance;

        #region Binding properties
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
        /// Property containing location type
        /// </summary>
        public string Type
        {
            get
            {
                string key = $"LocationType_{this.location.Type}";
                return Strings.ResourceManager.GetString(key) ?? "???";
            }
        }

        /// <summary>
        /// Property containing location latitude
        /// </summary>
        public string Latitude
        {
            get
            {
                return !this.location.MapLocation.Valid ? string.Empty :
                    GeoDataFormatter.FormatLatLong(this.location.MapLocation.Latitude, this.appSettings.CoordinateDisplayFormat);
            }
        }

        /// <summary>
        /// Property containing location longitude
        /// </summary>
        public string Longitude
        {
            get
            {
                return !this.location.MapLocation.Valid ? string.Empty :
                    GeoDataFormatter.FormatLatLong(this.location.MapLocation.Longitude, this.appSettings.CoordinateDisplayFormat);
            }
        }

        /// <summary>
        /// Property containing location altitude
        /// </summary>
        public string Altitude
        {
            get
            {
                return string.Format("{0} m", (int)this.location.MapLocation.Altitude.GetValueOrDefault(0.0));
            }
        }

        /// <summary>
        /// Property containing location distance
        /// </summary>
        public string Distance
        {
            get
            {
                return DataFormatter.FormatDistance(this.distance);
            }
        }

        /// <summary>
        /// Property containing location internet link
        /// </summary>
        public string InternetLink
        {
            get
            {
                return this.location.InternetLink;
            }
        }

        /// <summary>
        /// Property containing WebViewSource of location description
        /// </summary>
        public WebViewSource DescriptionWebViewSource
        {
            get; private set;
        }

        /// <summary>
        /// Command to execute when "refresh live waypoint" toolbar item is selected
        /// </summary>
        public ICommand RefreshLiveWaypointCommand { get; set; }

        /// <summary>
        /// Command to execute when "add tour plan location" toolbar item is selected
        /// </summary>
        public ICommand AddTourPlanLocationCommand { get; set; }

        /// <summary>
        /// Command to execute when "zoom to" menu item is selected on a location
        /// </summary>
        public ICommand ZoomToLocationCommand { get; set; }

        /// <summary>
        /// Command to execute when "set as compass target" menu item is selected on a location
        /// </summary>
        public ICommand SetAsCompassTargetCommand { get; set; }

        /// <summary>
        /// Command to execute when "navigate to" menu item is selected on a location
        /// </summary>
        public ICommand NavigateToLocationCommand { get; set; }

        /// <summary>
        /// Command to execute when "share" menu item is selected on a location
        /// </summary>
        public ICommand ShareLocationCommand { get; set; }

        /// <summary>
        /// Command to execute when "delete" menu item is selected on a location
        /// </summary>
        public ICommand DeleteLocationCommand { get; set; }

        /// <summary>
        /// Command to execute when user tapped on the internet link
        /// </summary>
        public ICommand? InternetLinkTappedCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model object based on the given location object
        /// </summary>
        /// <param name="appSettings">app settings object</param>
        /// <param name="location">location object</param>
        public LocationDetailsViewModel(AppSettings appSettings, Location location)
        {
            this.appSettings = appSettings;
            this.location = location;

            this.distance = 0.0;

            this.TypeImageSource =
                SvgImageCache.GetImageSource(location);

            this.DescriptionWebViewSource = new HtmlWebViewSource
            {
                Html = FormatLocationDescription(this.location),
                BaseUrl = "about:blank",
            };

            this.RefreshLiveWaypointCommand = new AsyncRelayCommand(this.OnRefreshLiveWaypoint);
            this.AddTourPlanLocationCommand = new AsyncRelayCommand(this.OnAddTourPlanLocationAsync);
            this.ZoomToLocationCommand = new AsyncRelayCommand(this.OnZoomToLocationAsync);
            this.SetAsCompassTargetCommand = new AsyncRelayCommand(this.OnSetAsCompassTargetAsync);
            this.NavigateToLocationCommand = new AsyncRelayCommand(this.OnNavigateToLocationAsync);
            this.ShareLocationCommand = new AsyncRelayCommand(this.OnShareLocationAsync);
            this.DeleteLocationCommand = new AsyncRelayCommand(this.OnDeleteLocationAsync);

            if (Uri.TryCreate(this.InternetLink, UriKind.Absolute, out Uri? _))
            {
                this.InternetLinkTappedCommand = new AsyncRelayCommand(this.OnInternetLinkTappedAsync);
            }

            Task.Run(this.UpdateDistance);
        }

        /// <summary>
        /// Formats location description
        /// </summary>
        /// <param name="location">location to format description</param>
        /// <returns>formatted description text</returns>
        private static string FormatLocationDescription(Location location)
        {
            string desc = HtmlConverter.FromHtmlOrMarkdown(location.Description);

            return HtmlConverter.AddTextColorStyles(
                desc,
                App.GetResourceColor("ElementTextColor", true),
                App.GetResourceColor("PageBackgroundColor", true),
                App.GetResourceColor("AccentColor", true));
        }

        /// <summary>
        /// Updates distance to current position
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task UpdateDistance()
        {
            var geolocationService = DependencyService.Get<IGeolocationService>();
            var position = await geolocationService.GetLastKnownPositionAsync();

            if (position != null)
            {
                this.distance = position.DistanceTo(this.location.MapLocation);

                this.OnPropertyChanged(nameof(this.Distance));
            }
        }

        /// <summary>
        /// Called when "Refresh live waypoint" toolbar button is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnRefreshLiveWaypoint()
        {
            try
            {
                var dataService = DependencyService.Get<IDataService>();
                var result = await dataService.GetLiveWaypointDataAsync(this.location.InternetLink);

                if (result.Data == null)
                {
                    return;
                }

                this.location.MapLocation =
                    new MapPoint(result.Data.Latitude, result.Data.Longitude, result.Data.Altitude);

                this.location.Description = result.Data.Description.Replace("\n", "<br/>");

                this.DescriptionWebViewSource = new HtmlWebViewSource
                {
                    Html = this.location.Description,
                    BaseUrl = "about:blank",
                };

                this.OnPropertyChanged(nameof(this.Latitude));
                this.OnPropertyChanged(nameof(this.Longitude));
                this.OnPropertyChanged(nameof(this.Altitude));
                this.OnPropertyChanged(nameof(this.Distance));
                this.OnPropertyChanged(nameof(this.DescriptionWebViewSource));

                // store new infos in location list
                var locationDataService = dataService.GetLocationDataService();

                Location? locationInList = await locationDataService.Get(this.location.Id);
                if (locationInList != null)
                {
                    locationInList.MapLocation = this.location.MapLocation;
                    locationInList.Description = this.location.Description;

                    await locationDataService.Update(locationInList);

                    var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
                    liveWaypointRefreshService.RemoveLiveWaypoint(locationInList.Id);
                    liveWaypointRefreshService.AddLiveWaypoint(locationInList);
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await UserInterface.DisplayAlert(
                    "Error while refreshing live waypoint: " + ex.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Called when "Add tour plan location" toolbar button is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnAddTourPlanLocationAsync()
        {
            var appMapService = DependencyService.Get<IAppMapService>();
            await appMapService.AddTourPlanLocation(this.location);
        }

        /// <summary>
        /// Called when "Zoom to" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToLocationAsync()
        {
            var appMapService = DependencyService.Get<IAppMapService>();
            await appMapService.UpdateLastShownPosition(this.location.MapLocation);

            appMapService.MapView.ZoomToLocation(this.location.MapLocation);

            await NavigationService.GoToMap();
        }

        /// <summary>
        /// Called when "Set as compass target" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnSetAsCompassTargetAsync()
        {
            var compassTarget = new CompassTarget
            {
                Title = this.location.Name,
                TargetLocation = this.location.MapLocation,
            };

            var appMapService = DependencyService.Get<IAppMapService>();
            await appMapService.SetCompassTarget(compassTarget);

            await NavigationService.GoToMap();
        }

        /// <summary>
        /// Called when "Navigate here" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnNavigateToLocationAsync()
        {
            var navigateLocation = new Microsoft.Maui.Devices.Sensors.Location(
                latitude: this.location.MapLocation.Latitude,
                longitude: this.location.MapLocation.Longitude);

            var options = new MapLaunchOptions
            {
                Name = this.location.Name,
                NavigationMode = NavigationMode.Driving,
            };

            await Map.OpenAsync(navigateLocation, options);
        }

        /// <summary>
        /// Called when "Share" menu item is selected
        /// </summary>
        /// <returns>task to wait for</returns>
        private async Task OnShareLocationAsync()
        {
            string text = "Share this location with...";
            string message = DataFormatter.FormatLocationShareText(this.location);

            await Share.RequestAsync(message, text);
        }

        /// <summary>
        /// Called when "Delete" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteLocationAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            var locationDataService = dataService.GetLocationDataService();

            await locationDataService.Remove(this.location.Id);

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.RemoveLiveWaypoint(this.location.Id);

            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.MapView.RemoveLocation(this.location.Id);

            await NavigationService.Instance.GoBack();

            UserInterface.DisplayToast("Selected location was deleted.");
        }

        /// <summary>
        /// Called when the user tapped on the internet link
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnInternetLinkTappedAsync()
        {
            await Browser.OpenAsync(
                new Uri(this.location.InternetLink),
                BrowserLaunchMode.External);
        }
    }
}
