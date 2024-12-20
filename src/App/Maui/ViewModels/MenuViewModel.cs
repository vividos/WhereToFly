﻿using System.Windows.Input;
using WhereToFly.App.Logic;
using WhereToFly.App.Services;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the menu appearing in the "hamburger menu" in the flyout page layout.
    /// </summary>
    public class MenuViewModel : ViewModelBase
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
            public PageKey PageKey { get; private set; }

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
            public MenuItemViewModel(string title, string svgImageName, PageKey pageKey)
            {
                this.Title = title;
                this.PageKey = pageKey;

                this.ImageSource = SvgImageCache.GetImageSource(svgImageName);
            }
        }

        /// <summary>
        /// Image source for app icon
        /// </summary>
        public ImageSource AppIcon { get; private set; }

        /// <summary>
        /// Version text to display under the app icon and name
        /// </summary>
        public string VersionText { get; private set; }

        /// <summary>
        /// List of menu items' view models
        /// </summary>
        public MenuItemViewModel[] MenuItemList { get; private set; }

        /// <summary>
        /// Currently selected menu item
        /// </summary>
        public MenuItemViewModel? SelectedMenuItem { get; set; }

        /// <summary>
        /// Command that is executed when this menu item has been tapped
        /// </summary>
        public ICommand MenuItemSelectedCommand { get; private set; }

        /// <summary>
        /// Creates a new menu view model object
        /// </summary>
        public MenuViewModel()
        {
            this.AppIcon = SvgImageCache.GetImageSource("applogo.svg");

            try
            {
                var version = Version.Parse(ThisAssembly.AssemblyVersion);
                this.VersionText = $"Version {version.ToString(3)}";
            }
            catch (Exception)
            {
                this.VersionText = "Unknown version";
            }

            this.MenuItemList =
            [
                new MenuItemViewModel("Map", "icons/map.svg", PageKey.MapPage),
                new MenuItemViewModel("Layers", "icons/layers-outline.svg", PageKey.LayerListPage),
                new MenuItemViewModel("Locations", "icons/format-list-bulleted.svg", PageKey.LocationListPage),
                new MenuItemViewModel("Tracks", "icons/map-marker-distance.svg", PageKey.TrackListPage),
                new MenuItemViewModel("Current Position", "icons/compass.svg", PageKey.CurrentPositionDetailsPage),
                new MenuItemViewModel("Weather", "icons/weather-partlycloudy.svg", PageKey.WeatherDashboardPage),
                new MenuItemViewModel("Settings", "icons/settings.svg", PageKey.SettingsPage),
                new MenuItemViewModel("Info", "icons/information-outline.svg", PageKey.InfoPage),
            ];

            this.MenuItemSelectedCommand = new Command(this.OnSelectedMenuItem);
        }

        /// <summary>
        /// Called when user tapped on a menu item
        /// </summary>
        private void OnSelectedMenuItem()
        {
            if (this.SelectedMenuItem == null)
            {
                return;
            }

            PageKey pageKey = this.SelectedMenuItem.PageKey;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // wait for complete app init before showing a page
                await App.InitializedTask;

                await NavigationService.Instance.NavigateAsync(pageKey, true);

                await Task.Delay(100);
                this.SelectedMenuItem = null;
                this.OnPropertyChanged(nameof(this.SelectedMenuItem));
            });
        }
    }
}
