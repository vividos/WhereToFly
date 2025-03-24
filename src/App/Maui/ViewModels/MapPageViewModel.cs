using System.Windows.Input;
using WhereToFly.App.Logic;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the map page
    /// </summary>
    internal class MapPageViewModel : ViewModelBase
    {
        /// <summary>
        /// App map service
        /// </summary>
        private readonly IAppMapService appMapService;

        /// <summary>
        /// Geolocation service to use for position updates
        /// </summary>
        private readonly IGeolocationService geolocationService;

        #region Binding properties

        /// <summary>
        /// Command to share my current location
        /// </summary>
        public ICommand ShareMyLocationCommand { get; private set; }
        #endregion

        /// <summary>
        /// Creates a new view model
        /// </summary>
        /// <param name="appMapService">app map service</param>
        /// <param name="geolocationService">geolocation service</param>
        public MapPageViewModel(
            IAppMapService appMapService,
            IGeolocationService geolocationService)
        {
            this.appMapService = appMapService;
            this.geolocationService = geolocationService;

            this.ShareMyLocationCommand = new AsyncCommand(this.OnShareMyLocation);
        }

        /// <summary>
        /// Called when the user clicked on the "Share position" link in the "my position" pin description.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnShareMyLocation()
        {
            var position =
                await this.geolocationService.GetPositionAsync(
                    timeout: TimeSpan.FromSeconds(0.1));

            if (position == null)
            {
                return;
            }

            var point = position.ToMapPoint();

            await this.appMapService.UpdateLastShownPosition(point);

            await Share.RequestAsync(
                DataFormatter.FormatMyPositionShareText(
                    point,
                    position.Timestamp),
                "Share my position with...");
        }
    }
}
