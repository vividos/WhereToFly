using System.Threading.Tasks;
using WhereToFly.Logic;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.ViewModels
{
    /// <summary>
    /// View model for a single location object
    /// </summary>
    public class LocationInfoViewModel
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
        /// Property containing detail infos for location
        /// </summary>
        public string DetailInfos
        {
            get
            {
                return string.Format(
                    "Type: {0}; Elevation: {1} m; Distance: {2}",
                    this.location.Type,
                    this.location.Elevation,
                    FormatDistance(this.location.Distance));
            }
        }

        /// <summary>
        /// Property containing location description
        /// </summary>
        public string Description { get; set; }

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
        public LocationInfoViewModel(LocationListViewModel parentViewModel, Location location)
        {
            this.parentViewModel = parentViewModel;
            this.location = location;

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
        /// Formats distance value as displayable text
        /// </summary>
        /// <param name="distance">distance in meter</param>
        /// <returns>displayable text</returns>
        public static string FormatDistance(double distance)
        {
            if (distance < 1e-6)
            {
                return "-";
            }

            if (distance < 1000.0)
            {
                return string.Format("{0} m", (int)distance);
            }

            return string.Format("{0:F1} km", distance / 1000.0);
        }
    }
}
