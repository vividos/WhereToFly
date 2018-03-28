using WhereToFly.Logic;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.ViewModels
{
    /// <summary>
    /// View model for the location details page
    /// </summary>
    public class LocationDetailsViewModel
    {
        /// <summary>
        /// App settings object
        /// </summary>
        private readonly AppSettings appSettings;

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
        /// Property containing location latitude
        /// </summary>
        public string Latitude
        {
            get
            {
                return !this.location.MapLocation.Valid ? string.Empty :
                    DataFormatter.FormatLatLong(this.location.MapLocation.Latitude, this.appSettings.CoordinateDisplayFormat);
            }
        }

        /// <summary>
        /// Property containing location longitude
        /// </summary>
        public string Longitude
        {
            get
            {
                return !this.location.MapLocation.Valid ? string.Empty :
                    DataFormatter.FormatLatLong(this.location.MapLocation.Longitude, this.appSettings.CoordinateDisplayFormat);
            }
        }

        /// <summary>
        /// Property containing location longitude
        /// </summary>
        public string Altitude
        {
            get
            {
                return string.Format("{0} m", (int)this.location.Elevation);
            }
        }

        /// <summary>
        /// Property containing location distance
        /// </summary>
        public string Distance
        {
            get
            {
                return DataFormatter.FormatDistance(this.location.Distance);
            }
        }

        /// <summary>
        /// Property containing location internet link
        /// </summary>
        public string InternetLink
        {
            get
            {
                return this.location.InternetLink;
            }
        }

        /// <summary>
        /// Property containing location description
        /// </summary>
        public WebViewSource DescriptionWebViewSource
        {
            get; private set;
        }

        /// <summary>
        /// Creates a new view model object based on the given location object
        /// </summary>
        /// <param name="appSettings">app settings object</param>
        /// <param name="location">location object</param>
        public LocationDetailsViewModel(AppSettings appSettings, Location location)
        {
            this.appSettings = appSettings;
            this.location = location;

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.DescriptionWebViewSource = new HtmlWebViewSource
            {
                Html = this.location.Description,
                BaseUrl = "about:blank"
            };
        }
    }
}
