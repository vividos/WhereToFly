using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Page to display location details
    /// </summary>
    public partial class LocationDetailsPage : ContentPage
    {
        /// <summary>
        /// View model for this page
        /// </summary>
        private readonly LocationDetailsViewModel viewModel;

        /// <summary>
        /// Creates new location details page
        /// </summary>
        /// <param name="location">location to display</param>
        public LocationDetailsPage(Location location)
        {
            this.Title = "Location details";

            this.InitializeComponent();

            this.BindingContext = this.viewModel = new LocationDetailsViewModel(App.Settings!, location);

            if (location.Type == LocationType.LiveWaypoint)
            {
                this.AddLiveWaypointRefreshToolbarButton();
            }

            this.AddTourPlanLocationToolbarButton();
        }

        /// <summary>
        /// Adds a "refresh live waypoint" button to the toolbar
        /// </summary>
        private void AddLiveWaypointRefreshToolbarButton()
        {
            var refreshLiveWaypointButton = new ToolbarItem(
                "Refresh live waypoint",
                "refresh.png",
                this.OnClicked_ToolbarButtonRefreshLiveWaypoint,
                ToolbarItemOrder.Primary)
            {
                AutomationId = "RefreshLiveWaypoint",
            };

            this.ToolbarItems.Add(refreshLiveWaypointButton);
        }

        /// <summary>
        /// Called when user clicked on the "refresh live waypoint" toolbar button.
        /// </summary>
        private void OnClicked_ToolbarButtonRefreshLiveWaypoint()
        {
            this.viewModel.RefreshLiveWaypointCommand.Execute(null);
        }

        /// <summary>
        /// Adds a "Add tour plan location" button to the toolbar
        /// </summary>
        private void AddTourPlanLocationToolbarButton()
        {
            var addTourPlanLocationButton = new ToolbarItem(
                "Add tour plan location",
                "map_marker_plus.png",
                this.OnClicked_ToolbarButtonAddTourPlanLocation,
                ToolbarItemOrder.Secondary)
            {
                AutomationId = "AddTourPlanLocation",
            };

            this.ToolbarItems.Add(addTourPlanLocationButton);
        }

        /// <summary>
        /// Called when user clicked on the "add plan tour location" toolbar button.
        /// </summary>
        private void OnClicked_ToolbarButtonAddTourPlanLocation()
        {
            this.viewModel.AddTourPlanLocationCommand.Execute(null);
        }
    }
}
