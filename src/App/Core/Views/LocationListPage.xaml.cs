using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
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

            this.BindingContext = this.viewModel = new LocationListViewModel(App.Settings);

            this.SetupToolbar();

            Task.Run(this.InitPositionAsync);
        }

        /// <summary>
        /// Sets up toolbar for this page
        /// </summary>
        private void SetupToolbar()
        {
            this.AddImportLocationsToolbarButton();
            this.AddDeleteLocationListToolbarButton();
        }

        /// <summary>
        /// Adds "Import locations" toolbar button
        /// </summary>
        private void AddImportLocationsToolbarButton()
        {
            var importLocationsButton = new ToolbarItem(
                "Import locations",
                Converter.ImagePathConverter.GetDeviceDependentImage("playlist_plus"),
                this.OnClicked_ToolbarButtonImportLocations,
                ToolbarItemOrder.Primary)
            {
                AutomationId = "ImportLocations",
            };

            this.ToolbarItems.Add(importLocationsButton);
        }

        /// <summary>
        /// Called when toolbar button "Import locations" was clicked
        /// </summary>
        private void OnClicked_ToolbarButtonImportLocations()
        {
            this.viewModel.ImportLocationsCommand.Execute(null);
        }

        /// <summary>
        /// Adds "Delete location list" toolbar button
        /// </summary>
        private void AddDeleteLocationListToolbarButton()
        {
            var deleteLocationListButton = new ToolbarItem(
                "Delete location list",
                Converter.ImagePathConverter.GetDeviceDependentImage("delete_forever"),
                async () => await this.OnClicked_ToolbarButtonDeleteLocationList(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "DeleteLocationList",
            };

            this.ToolbarItems.Add(deleteLocationListButton);
        }

        /// <summary>
        /// Called when toolbar button "Delete location list" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonDeleteLocationList()
        {
            await this.viewModel.ClearLocationsAsync();
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
