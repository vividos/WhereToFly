using WhereToFly.Logic;
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
        /// Property containing the location object
        /// </summary>
        public Location Location => this.location;

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
        /// Property containing detail infos for location
        /// </summary>
        public string DetailInfos
        {
            get
            {
                return string.Format(
                    "Type: {0}; Elevation: {1} m; Distance: {2}",
                    this.location.Type,
                    this.location.Elevation,
                    FormatDistance(this.location.Distance));
            }
        }

        /// <summary>
        /// Property containing location description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates a new view model object based on the given location object
        /// </summary>
        /// <param name="location">location object</param>
        public LocationInfoViewModel(Location location)
        {
            this.location = location;
            this.Description = HtmlConverter.StripAllTags(this.location.Description);
        }

        /// <summary>
        /// Formats distance value as displayable text
        /// </summary>
        /// <param name="distance">distance in meter</param>
        /// <returns>displayable text</returns>
        public static string FormatDistance(double distance)
        {
            if (distance < 1e-6)
            {
                return "-";
            }

            if (distance < 1000.0)
            {
                return string.Format("{0} m", (int)distance);
            }

            return string.Format("{0:F1} km", distance / 1000.0);
        }
    }
}
