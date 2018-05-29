using System;
using System.IO;
using System.Text;
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
            /// Lazy-loading backing store for type image source
            /// </summary>
            private readonly Lazy<ImageSource> imageSource;

            /// <summary>
            /// Title text of the menu item
            /// </summary>
            public string Title { get; private set; }

            /// <summary>
            /// Page key of page to navigate to when user clicked on the menu item
            /// </summary>
            public string PageKey { get; private set; }

            /// <summary>
            /// Image name of SVG image
            /// </summary>
            private readonly string svgImageName;

            /// <summary>
            /// Returns image source for SvgImage in order to display the type image
            /// </summary>
            public ImageSource ImageSource
            {
                get
                {
                    return this.imageSource.Value;
                }
            }

            /// <summary>
            /// Creates a new menu item view model object
            /// </summary>
            /// <param name="title">title of menu item</param>
            /// <param name="svgImageName">SVG image name of image to display</param>
            /// <param name="pageKey">page key of page to navigate to</param>
            public MenuItemViewModel(string title, string svgImageName, string pageKey)
            {
                this.Title = title;
                this.svgImageName = svgImageName;
                this.PageKey = pageKey;

                this.imageSource = new Lazy<ImageSource>(this.GetImageSource);
            }

            /// <summary>
            /// Returns image source for SVG image
            /// </summary>
            /// <returns>image source, or null when no icon could be found</returns>
            private ImageSource GetImageSource()
            {
                string svgText = DependencyService.Get<SvgImageCache>()
                    .GetSvgImage(this.svgImageName, "#000000");

                if (svgText != null)
                {
                    return ImageSource.FromStream(() => new MemoryStream(Encoding.UTF8.GetBytes(svgText)));
                }

                return null;
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
            var platform = DependencyService.Get<IPlatform>();
            this.VersionText = "Version " + platform.AppVersionNumber;

            this.MenuItemList = new MenuItemViewModel[]
            {
                new MenuItemViewModel("Map", "icons/map.svg", Constants.PageKeyMapPage),
                new MenuItemViewModel("Location List", "icons/format-list-bulleted.svg", Constants.PageKeyLocationListPage),
                new MenuItemViewModel("Current Position", "icons/compass.svg", Constants.PageKeyCurrentPositionDetailsPage),
                new MenuItemViewModel("Settings", "icons/settings.svg", Constants.PageKeySettingsPage),
                new MenuItemViewModel("Info", "icons/information-outline.svg", Constants.PageKeyInfoPage),
            };
        }
    }
}
