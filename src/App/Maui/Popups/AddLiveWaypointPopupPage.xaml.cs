using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Popup page for adding a live waypoint and edit its properties. Reports back edited live
    /// waypoint location, or null to cancel adding live waypoint.
    /// </summary>
    public partial class AddLiveWaypointPopupPage : BasePopupPage<Location>
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly AddLiveWaypointPopupViewModel viewModel;

        /// <summary>
        /// Creates a new popup page to edit live waypoint properties
        /// </summary>
        /// <param name="liveWaypoint">live waypoint to edit</param>
        public AddLiveWaypointPopupPage(Location liveWaypoint)
        {
            this.InitializeComponent();

            this.BindingContext = this.viewModel = new AddLiveWaypointPopupViewModel(liveWaypoint);
        }

        /// <summary>
        /// Called when user clicked on the "Add track" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedAddLiveWaypointButton(object? sender, EventArgs args)
        {
            this.SetResult(this.viewModel.LiveWaypoint);
        }
    }
}
