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
        private bool reloadLocationListOnAppearing;

        /// <summary>
        /// Creates a new location list page
        /// </summary>
        public LocationListPage()
        {
            this.Title = "Location list";

            this.InitializeComponent();

            this.geolocator = DependencyService.Get<GeolocationService>().Geolocator;

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
            ToolbarItem importLocationsButton = new ToolbarItem(
                "Import locations",
                Converter.ImagePathConverter.GetDeviceDependentImage("playlist_plus"),
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
            this.reloadLocationListOnAppearing = true;
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyImportLocationsPage, animated: true);
        }

        /// <summary>
        /// Adds "Delete location list" toolbar button
        /// </summary>
        private void AddDeleteLocationListToolbarButton()
        {
            ToolbarItem deleteLocationListButton = new ToolbarItem(
                "Delete location list",
                Converter.ImagePathConverter.GetDeviceDependentImage("delete_forever"),
                async () => await this.OnClicked_ToolbarButtonDeleteLocationList(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "DeleteLocationList"
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

        /// <summary>
        /// Called when the binding context of the view cell has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnViewCellBindingContextChanged(object sender, EventArgs args)
        {
            base.OnBindingContextChanged();

            ViewCell viewCell = (ViewCell)sender;
            var cellViewModel = viewCell.BindingContext as LocationListEntryViewModel;

            if (cellViewModel != null &&
                cellViewModel.Location.IsPlanTourLocation)
            {
                viewCell.ContextActions.Add(
                    new MenuItem
                    {
                        Text = "Add tour plan location position",
                        Icon = new FileImageSource
                        {
                            File = Converter.ImagePathConverter.GetDeviceDependentImage("map_marker_plus")
                        },
                        Command = this.viewModel.AddTourPlanLocationCommand,
                        CommandParameter = cellViewModel.Location,
                        AutomationId = "AddTourPlanLocation"
                    });
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

                Task.Run(this.viewModel.ReloadLocationListAsync);
            }
            else
            {
                Task.Run(this.viewModel.CheckReloadNeeded);
            }

            Task.Run(async () =>
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
