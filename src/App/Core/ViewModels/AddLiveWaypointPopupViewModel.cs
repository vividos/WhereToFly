using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "add live waypoint" popup page
    /// </summary>
    public class AddLiveWaypointPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// Live waypoint being edited
        /// </summary>
        private readonly Location liveWaypoint;

        #region Binding properties
        /// <summary>
        /// Property containing the live waypoint name
        /// </summary>
        public string Name
        {
            get => this.liveWaypoint.Name;
            set
            {
                this.liveWaypoint.Name = value;
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
                var uri = new AppResourceUri(this.liveWaypoint.InternetLink);
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
            this.liveWaypoint = liveWaypoint;
        }
    }
}
