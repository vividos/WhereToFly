using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display compass details, such as compass rose, direction, distance etc.
    /// </summary>
    public partial class CompassDetailsPage : ContentPage
    {
        /// <summary>
        /// View model for the compass details page
        /// </summary>
        private readonly CompassDetailsViewModel viewModel;

        /// <summary>
        /// Geolocation service to use for position updates
        /// </summary>
        private readonly IGeolocationService geolocationService;

        /// <summary>
        /// Creates new current position details page
        /// </summary>
        public CompassDetailsPage()
        {
            this.InitializeComponent();

            this.geolocationService = DependencyService.Get<IGeolocationService>();

            this.BindingContext = this.viewModel = new CompassDetailsViewModel(App.Settings);

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
