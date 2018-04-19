using WhereToFly.App.Logic.Model;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the edit location details page
    /// </summary>
    public class EditLocationDetailsViewModel
    {
        /// <summary>
        /// App settings object
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        /// Location object to edit
        /// </summary>
        private readonly Location location;

        /// <summary>
        /// Creates a new edit location details view model
        /// </summary>
        /// <param name="appSettings">app settings to use</param>
        /// <param name="location">location object to edit</param>
        public EditLocationDetailsViewModel(AppSettings appSettings, Location location)
        {
            this.appSettings = appSettings;
            this.location = location;
        }
    }
}
