using System;
using System.IO;
using System.Threading.Tasks;
using WhereToFly.App.Geo.Spatial;
using WhereToFly.App.Logic;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
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
        /// Lazy-loading backing store for type image source
        /// </summary>
        private readonly Lazy<ImageSource> typeImageSource;

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
        public ImageSource TypeImageSource
        {
            get
            {
                return this.typeImageSource.Value;
            }
        }

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
        /// Command to execute when "show details" context action is selected on a location
        /// </summary>
        public Command ShowDetailsLocationContextAction { get; set; }

        /// <summary>
        /// Command to execute when "zoom to" context action is selected on a location
        /// </summary>
        public Command ZoomToLocationContextAction { get; set; }

        /// <summary>
        /// Command to execute when "delete" context action is selected on a location
        /// </summary>
        public Command DeleteLocationContextAction { get; set; }

        /// <summary>
        /// Creates a new view model object based on the given location object
        /// </summary>
        /// <param name="parentViewModel">parent view model</param>
        /// <param name="location">location object</param>
        /// <param name="myCurrentPosition">the user's current position; may be null</param>
        public LocationListEntryViewModel(LocationListViewModel parentViewModel, Location location, MapPoint myCurrentPosition)
        {
            this.parentViewModel = parentViewModel;
            this.location = location;

            this.Distance = myCurrentPosition != null ? myCurrentPosition.DistanceTo(this.location.MapLocation) : 0.0;

            this.typeImageSource = new Lazy<ImageSource>(this.GetTypeImageSource);

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.Description = HtmlConverter.StripAllTags(this.location.Description);

            this.ShowDetailsLocationContextAction =
                new Command(async () => await this.OnShowDetailsLocation());

            this.ZoomToLocationContextAction =
                new Command(async () => await this.OnZoomToLocationAsync());

            this.DeleteLocationContextAction =
                new Command(async () => await this.OnDeleteLocationAsync());
        }

        /// <summary>
        /// Called when "show details" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnShowDetailsLocation()
        {
            await this.parentViewModel.NavigateToLocationDetails(this.location);
        }

        /// <summary>
        /// Called when "zoom to" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToLocationAsync()
        {
            await this.parentViewModel.ZoomToLocation(this.location);
        }

        /// <summary>
        /// Called when "delete" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteLocationAsync()
        {
            await this.parentViewModel.DeleteLocation(this.location);
        }

        /// <summary>
        /// Returns type icon from location type
        /// </summary>
        /// <returns>image source, or null when no icon could be found</returns>
        private ImageSource GetTypeImageSource()
        {
            string svgText = DependencyService.Get<SvgImageCache>()
                .GetSvgImageByLocationType(this.location.Type, "#000000");

            if (svgText != null)
            {
                return ImageSource.FromStream(() => new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgText)));
            }

            return null;
        }
    }
}
