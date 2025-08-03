using WhereToFly.App.Abstractions;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Page to display location list; the list can be filtered by a filter text, and a single
    /// location entry can be tapped to get more infos.
    /// </summary>
    public partial class LocationListPage : ContentPage
    {
        /// <summary>
        /// View model for the location list page
        /// </summary>
        private readonly LocationListViewModel viewModel;

        /// <summary>
        /// Geolocation service to use for position updates
        /// </summary>
        private readonly IGeolocationService geolocationService;

        /// <summary>
        /// Indicates if import page was started; used to update location list when returning to
        /// this page.
        /// </summary>
        private bool reloadLocationListOnAppearing;

        /// <summary>
        /// Creates a new location list page
        /// </summary>
        public LocationListPage()
        {
            this.Title = "Location list";

            this.InitializeComponent();

            this.geolocationService = DependencyService.Get<IGeolocationService>();

            this.BindingContext = this.viewModel = new LocationListViewModel(App.Settings!);

            Task.Run(this.InitPositionAsync);
        }

        /// <summary>
        /// Get initial position and show it. Waits for 1 second; when no position is found, just
        /// waits for regular PositionChanged events from Geolocator.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitPositionAsync()
        {
            var position =
                await this.geolocationService.GetPositionAsync(timeout: TimeSpan.FromSeconds(1));

            if (position != null)
            {
                this.viewModel.OnPositionChanged(
                    this,
                    new GeolocationEventArgs(position));
            }
        }

        #region Page lifecycle methods
        /// <summary>
        /// Called when page is appearing; get current position
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (this.reloadLocationListOnAppearing)
            {
                this.reloadLocationListOnAppearing = false;

                this.viewModel.UpdateLocationList();
            }
            else
            {
                Task.Run(this.viewModel.CheckReloadNeeded);
            }

            Task.Run(async () =>
            {
                var position = await this.geolocationService.GetPositionAsync(TimeSpan.FromSeconds(1));
                if (position != null)
                {
                    this.viewModel.OnPositionChanged(this, new GeolocationEventArgs(position));
                }
            });
        }
        #endregion
    }
}
