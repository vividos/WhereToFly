using System.Collections.Generic;
using WhereToFly.App.Model;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the weather dashboard page
    /// </summary>
    internal class WeatherDashboardViewModel
    {
        /// <summary>
        /// List of weather icon descriptions for all weather icons to display
        /// </summary>
        public List<WeatherIconDescription> WeatherIconDescriptionList { get; set; }

        /// <summary>
        /// Creates a new view model for the weather dashboard
        /// </summary>
        public WeatherDashboardViewModel()
        {
            this.WeatherIconDescriptionList = new List<WeatherIconDescription>
            {
                new WeatherIconDescription
                {
                    Name = "DHV Wetter",
                    Type = WeatherIconDescription.IconType.IconLink,
                    WebLink = "https://www.dhv.de/wetter/"
                },
                new WeatherIconDescription
                {
                    Name = "Lightningmaps.org",
                    Type = WeatherIconDescription.IconType.IconLink,
                    WebLink = "https://www.lightningmaps.org/"
                },
                new WeatherIconDescription
                {
                    Name = "Windy",
                    Type = WeatherIconDescription.IconType.IconApp,
                    WebLink = "com.windyty.android"
                },
                new WeatherIconDescription
                {
                    Name = "WetterOnline",
                    Type = WeatherIconDescription.IconType.IconApp,
                    WebLink = "de.wetteronline.wetterapp"
                },
                ////new WeatherIconDescription
                ////{
                ////    Name = "Add new...",
                ////    Type = WeatherIconDescription.IconType.IconPlaceholder,
                ////},
            };
        }
    }
}
