using Plugin.Geolocator.Abstractions;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display location list; the list can be filtered by a filter text, and a single
    /// location entry can be tapped to get more infos.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationListPage : ContentPage
    {
        /// <summary>
        /// View model for the location list page
        /// </summary>
        private readonly LocationListViewModel viewModel;

        /// <summary>
        /// Geo locator to use for position updates
        /// </summary>
        private readonly IGeolocator geolocator;

        /// <summary>
        /// Indicates if import page was started; used to update location list when returning to
        /// this page.
        /// </summary>
        private bool startedImportPage;

        /// <summary>
        /// Creates a new location list page
        /// </summary>
        public LocationListPage()
        {
            this.Title = "Location list";

            this.InitializeComponent();

            this.geolocator = Plugin.Geolocator.CrossGeolocator.Current;

            this.BindingContext = this.viewModel = new LocationListViewModel(App.Settings);

            this.SetupToolbar();

            Task.Factory.StartNew(this.InitPositionAsync);
        }

        /// <summary>
        /// Sets up toolbar for this page
        /// </summary>
        private void SetupToolbar()
        {
            this.AddImportLocationsToolbarButton();
        }

        /// <summary>
        /// Adds "Import locations" toolbar button
        /// </summary>
        private void AddImportLocationsToolbarButton()
        {
            ToolbarItem importLocationsButton = new ToolbarItem(
                "Import locations",
                "playlist_plus.xml",
                async () => await this.OnClicked_ToolbarButtonImportLocations(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "ImportLocations"
            };

            this.ToolbarItems.Add(importLocationsButton);
        }

        /// <summary>
        /// Called when toolbar button "Import locations" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonImportLocations()
        {
            this.startedImportPage = true;
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyImportLocationsPage, animated: true);
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
        /// Called when an item was tapped on the location list
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnItemTapped_LocationsListView(object sender, ItemTappedEventArgs args)
        {
            var localViewModel = this.BindingContext as LocationListViewModel;

            var locationListEntryViewModel = args.Item as LocationListEntryViewModel;
            localViewModel.ItemTappedCommand.Execute(locationListEntryViewModel.Location);
        }

        #region Page lifecycle methods
        /// <summary>
        /// Called when page is appearing; get current position
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (this.startedImportPage)
            {
                this.startedImportPage = false;

                Task.Factory.StartNew(this.viewModel.ReloadLocationListAsync);
            }

            Task.Factory.StartNew(async () =>
            {
                var position = await this.geolocator.GetPositionAsync(TimeSpan.FromSeconds(1));
                if (position != null)
                {
                    this.viewModel.OnPositionChanged(this, new PositionEventArgs(position));
                }
            });
        }
        #endregion
    }
}
