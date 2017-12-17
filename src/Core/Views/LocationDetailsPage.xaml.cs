using Plugin.Geolocator.Abstractions;
using System.Threading.Tasks;
using WhereToFly.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.Core.Views
{
    /// <summary>
    /// Page to display location details, such as coordinates, speed, heading and accuracy
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationDetailsPage : ContentPage
    {
        /// <summary>
        /// View model for the location details page
        /// </summary>
        private readonly LocationDetailsViewModel viewModel;

        /// <summary>
        /// Geo locator to use for position updates
        /// </summary>
        private readonly IGeolocator geolocator;

        /// <summary>
        /// Creates new location details page
        /// </summary>
        public LocationDetailsPage()
        {
            this.InitializeComponent();

            this.geolocator = Plugin.Geolocator.CrossGeolocator.Current;

            this.BindingContext = this.viewModel = new LocationDetailsViewModel();

            this.SetupToolbar();
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
            // TODO
            await Task.Delay(0);
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
