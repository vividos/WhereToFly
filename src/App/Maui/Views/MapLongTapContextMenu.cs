using CommunityToolkit.Maui.Views;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;
using WhereToFly.App.Popups;
using WhereToFly.App.ViewModels;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Views
{
    /// <summary>
    /// Context menu for a long-tap on a map point
    /// </summary>
    internal static class MapLongTapContextMenu
    {
        /// <summary>
        /// Context menu result
        /// </summary>
        public enum Result
        {
            /// <summary>
            /// User selected menu item to add a new waypoint
            /// </summary>
            AddNewWaypoint,

            /// <summary>
            /// User selected menu item to set location as compass target
            /// </summary>
            SetAsCompassTarget,

            /// <summary>
            /// User selected menu item to navigate here
            /// </summary>
            NavigateHere,

            /// <summary>
            /// User selected menu item to show the flying range cone
            /// </summary>
            ShowFlyingRange,

            /// <summary>
            /// User cancelled the context menu
            /// </summary>
            Cancel,
        }

        /// <summary>
        /// Shows the context menu
        /// </summary>
        /// <param name="point">map point where the long-tap occurred</param>
        /// <param name="appSettings">app settings</param>
        /// <returns>task to wait on</returns>
        public static async ValueTask<Result> ShowAsync(MapPoint point, AppSettings appSettings)
        {
            string latitudeText = GeoDataFormatter.FormatLatLong(point.Latitude, appSettings.CoordinateDisplayFormat);
            string longitudeText = GeoDataFormatter.FormatLatLong(point.Longitude, appSettings.CoordinateDisplayFormat);

            string caption =
                $"Selected point at Latitude: {latitudeText}, Longitude: {longitudeText}, Altitude {point.Altitude.GetValueOrDefault(0.0)} m";

            ContextMenuPopupPage? popupPage = null;

            var items = new List<MenuItem>
            {
                new MenuItem
                {
                    Text = "Add new waypoint",
                    IconImageSource = SvgImageCache.GetImageSource("info/images/playlist-plus.svg"),
                    Command = new Command(() => popupPage?.Close(Result.AddNewWaypoint)),
                },
                new MenuItem
                {
                    Text = "Set as compass target",
                    IconImageSource = SvgImageCache.GetImageSource("weblib/images/compass-rose.svg"),
                    Command = new Command(() => popupPage?.Close(Result.SetAsCompassTarget)),
                },
                new MenuItem
                {
                    Text = "Navigate here",
                    IconImageSource = SvgImageCache.GetImageSource("info/images/directions.svg"),
                    Command = new Command(() => popupPage?.Close(Result.NavigateHere)),
                },
                new MenuItem
                {
                    Text = "Show flying range",
                    IconImageSource = SvgImageCache.GetImageSource("info/images/arrow-expand-horizontal.svg"),
                    Command = new Command(() => popupPage?.Close(Result.ShowFlyingRange)),
                },
            };

            var viewModel = new ContextMenuPopupViewModel(
                caption,
                items,
                () => { });

            popupPage = new ContextMenuPopupPage(viewModel);

            var mainPage = App.Current?.MainPage
                ?? throw new InvalidOperationException("MainPage is not available");

            object? result = await mainPage.ShowPopupAsync(popupPage);

            return (Result?)result ?? Result.Cancel;
        }
    }
}
