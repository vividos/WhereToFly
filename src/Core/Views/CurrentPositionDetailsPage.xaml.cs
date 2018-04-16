using Plugin.Geolocator.Abstractions;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Logic;
using WhereToFly.App.Logic.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display current position details, such as coordinates, speed, heading and accuracy
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentPositionDetailsPage : ContentPage
    {
        /// <summary>
        /// View model for the current position details page
        /// </summary>
        private readonly CurrentPositionDetailsViewModel viewModel;

        /// <summary>
        /// Geo locator to use for position updates
        /// </summary>
        private readonly IGeolocator geolocator;

        /// <summary>
        /// Creates new current position details page
        /// </summary>
        public CurrentPositionDetailsPage()
        {
            this.Title = "Current position";

            this.InitializeComponent();

            this.geolocator = Plugin.Geolocator.CrossGeolocator.Current;

            this.BindingContext = this.viewModel = new CurrentPositionDetailsViewModel(App.Settings);

            this.SetupToolbar();

            Task.Factory.StartNew(this.InitPositionAsync);
        }

        /// <summary>
        /// Get initial position and show it. Waits for 1 second; when no position is found, just
        /// waits for regular PositionChanged events from Geolocator.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitPositionAsync()
        {
            var position =
                await this.geolocator.GetPositionAsync(timeout: TimeSpan.FromSeconds(1), includeHeading: false);

            if (position != null)
            {
                this.viewModel.OnPositionChanged(
                    this,
                    new PositionEventArgs(position));
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
                "share_variant.xml",
                async () => await this.OnClicked_ToolbarButtonSharePosition(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "SharePosition"
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
                await this.geolocator.GetPositionAsync(timeout: TimeSpan.FromSeconds(0.1), includeHeading: false);

            if (position != null)
            {
                var point = new MapPoint(position.Latitude, position.Longitude);

                await CrossShare.Current.Share(
                    new ShareMessage
                    {
                        Title = Constants.AppTitle,
                        Text = DataFormatter.FormatMyPositionShareText(point, position.Altitude, position.Timestamp)
                    },
                    new ShareOptions
                    {
                        ChooserTitle = "Share my position with..."
                    });
            }
        }

        #region Page lifecycle methods
        /// <summary>
        /// Called when page is appearing; start position updates
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            Task.Factory.StartNew(async () =>
            {
                await this.geolocator.StartListeningAsync(
                    Constants.GeoLocationMinimumTimeForUpdate,
                    Constants.GeoLocationMinimumDistanceForUpdateInMeters,
                    includeHeading: true);
            });

            this.geolocator.PositionChanged += this.viewModel.OnPositionChanged;
        }

        /// <summary>
        /// Called when form is disappearing; stop position updates
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            this.geolocator.PositionChanged -= this.viewModel.OnPositionChanged;

            Task.Factory.StartNew(async () =>
            {
                await this.geolocator.StopListeningAsync();
            });
        }
        #endregion
    }
}
