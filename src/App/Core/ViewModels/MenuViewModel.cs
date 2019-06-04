using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the menu appearing in the "hamburger menu" in the master detail layout.
    /// </summary>
    public class MenuViewModel
    {
        /// <summary>
        /// View model for a single menu item
        /// </summary>
        public class MenuItemViewModel
        {
            /// <summary>
            /// Title text of the menu item
            /// </summary>
            public string Title { get; private set; }

            /// <summary>
            /// Page key of page to navigate to when user clicked on the menu item
            /// </summary>
            public string PageKey { get; private set; }

            /// <summary>
            /// Returns image source for SvgImage in order to display the type image
            /// </summary>
            public ImageSource ImageSource { get; }

            /// <summary>
            /// Creates a new menu item view model object
            /// </summary>
            /// <param name="title">title of menu item</param>
            /// <param name="svgImageName">SVG image name of image to display</param>
            /// <param name="pageKey">page key of page to navigate to</param>
            public MenuItemViewModel(string title, string svgImageName, string pageKey)
            {
                this.Title = title;
                this.PageKey = pageKey;

                this.ImageSource = SvgImageCache.GetImageSource(svgImageName, "#000000");
            }
        }

        /// <summary>
        /// Version text to display under the app icon and name
        /// </summary>
        public string VersionText { get; private set; }

        /// <summary>
        /// List of menu items' view models
        /// </summary>
        public MenuItemViewModel[] MenuItemList { get; private set; }

        /// <summary>
        /// Creates a new menu view model object
        /// </summary>
        public MenuViewModel()
        {
            try
            {
                this.VersionText = $"Version {AppInfo.VersionString} (Build {AppInfo.BuildString})";
            }
            catch (Exception)
            {
                this.VersionText = "Unknown version";
            }

            this.MenuItemList = new MenuItemViewModel[]
            {
                new MenuItemViewModel("Map", "icons/map.svg", Constants.PageKeyMapPage),
                new MenuItemViewModel("Layers", "icons/layers-outline.svg", Constants.PageKeyLayerListPage),
                new MenuItemViewModel("Locations", "icons/format-list-bulleted.svg", Constants.PageKeyLocationListPage),
                new MenuItemViewModel("Tracks", "icons/map-marker-distance.svg", Constants.PageKeyTrackListPage),
                new MenuItemViewModel("Current Position", "icons/compass.svg", Constants.PageKeyCurrentPositionDetailsPage),
                new MenuItemViewModel("Weather", "icons/weather-partlycloudy.svg", Constants.PageKeyWeatherDashboardPage),
                new MenuItemViewModel("Settings", "icons/settings.svg", Constants.PageKeySettingsPage),
                new MenuItemViewModel("Info", "icons/information-outline.svg", Constants.PageKeyInfoPage),
            };
        }
    }
}
