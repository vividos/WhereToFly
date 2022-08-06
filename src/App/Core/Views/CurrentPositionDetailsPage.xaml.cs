using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.Logic;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display current position details, such as coordinates, speed, heading and accuracy
    /// </summary>
    public partial class CurrentPositionDetailsPage : ContentPage
    {
        /// <summary>
        /// View model for the current position details page
        /// </summary>
        private readonly CurrentPositionDetailsViewModel viewModel;

        /// <summary>
        /// Geolocation service to use for position updates
        /// </summary>
        private readonly IGeolocationService geolocationService;

        /// <summary>
        /// Creates new current position details page
        /// </summary>
        public CurrentPositionDetailsPage()
        {
            this.InitializeComponent();

            this.geolocationService = DependencyService.Get<IGeolocationService>();

            this.BindingContext = this.viewModel = new CurrentPositionDetailsViewModel(App.Settings);

            this.SetupToolbar();

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

        /// <summary>
        /// Sets up toolbar for this page
        /// </summary>
        private void SetupToolbar()
        {
            this.AddSharePositionToolbarButton();
        }

        /// <summary>
        /// Adds a "Share position" button to the toolbar
        /// </summary>
        private void AddSharePositionToolbarButton()
        {
            ToolbarItem sharePositionButton = new ToolbarItem(
                "Share position",
                Converter.ImagePathConverter.GetDeviceDependentImage("share_variant"),
                async () => await this.OnClicked_ToolbarButtonSharePosition(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "SharePosition",
            };

            this.ToolbarItems.Add(sharePositionButton);
        }

        /// <summary>
        /// Called when toolbar button "Share" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonSharePosition()
        {
            var position =
                await this.geolocationService.GetPositionAsync(timeout: TimeSpan.FromSeconds(0.1));

            if (position != null)
            {
                var point = new MapPoint(position.Latitude, position.Longitude, position.Altitude);

                await App.ShareMessageAsync(
                    "Share my position with...",
                    DataFormatter.FormatMyPositionShareText(point, position.Timestamp));
            }
        }

        #region Page lifecycle methods
        /// <summary>
        /// Called when page is appearing; start position updates
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            this.geolocationService.PositionChanged += this.viewModel.OnPositionChanged;

            this.viewModel.StartCompass();

            await this.InitPositionAsync();
        }

        /// <summary>
        /// Called when form is disappearing; stop position updates
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            this.viewModel.StopCompass();

            this.geolocationService.PositionChanged -= this.viewModel.OnPositionChanged;
        }
        #endregion
    }
}
