using WhereToFly.Logic.Model;

namespace WhereToFly.Core.ViewModels
{
    /// <summary>
    /// View model for a single location object
    /// </summary>
    public class LocationInfoViewModel
    {
        /// <summary>
        /// Location to show
        /// </summary>
        private readonly Location location;

        /// <summary>
        /// Property containing location name
        /// </summary>
        public string Name
        {
            get
            {
                return this.location.Name;
            }
        }

        /// <summary>
        /// Property containing location description
        /// </summary>
        public string Description
        {
            get
            {
                return this.location.Description;
            }
        }

        /// <summary>
        /// Creates a new view model object based on the given location object
        /// </summary>
        /// <param name="location">location object</param>
        public LocationInfoViewModel(Location location)
        {
            this.location = location;
        }
    }
}
