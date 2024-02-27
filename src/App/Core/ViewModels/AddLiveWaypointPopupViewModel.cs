using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the "add live waypoint" popup page
    /// </summary>
    public class AddLiveWaypointPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// Live waypoint being edited
        /// </summary>
        public Location LiveWaypoint { get; }

        #region Binding properties
        /// <summary>
        /// Property containing the live waypoint name
        /// </summary>
        public string Name
        {
            get => this.LiveWaypoint.Name;
            set
            {
                this.LiveWaypoint.Name = value;
                this.OnPropertyChanged(nameof(this.Name));
            }
        }

        /// <summary>
        /// Property containing the live waypoint type
        /// </summary>
        public string Type
        {
            get
            {
                var uri = new AppResourceUri(this.LiveWaypoint.InternetLink);
                return uri.Type.ToString();
            }
        }
        #endregion

        /// <summary>
        /// Creates a new "add live waypoint" popup page view model
        /// </summary>
        /// <param name="liveWaypoint">live waypoint to edit</param>
        public AddLiveWaypointPopupViewModel(Location liveWaypoint)
        {
            this.LiveWaypoint = liveWaypoint;
        }
    }
}
