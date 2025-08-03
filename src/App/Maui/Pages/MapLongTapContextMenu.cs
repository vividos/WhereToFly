using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;
using WhereToFly.App.Popups;
using WhereToFly.App.ViewModels;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Pages
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
            /// User selected menu item to plan a tour with this poistion
            /// </summary>
            PlanTour,

            /// <summary>
            /// User selected menu item to find flights at this position
            /// </summary>
            FindFlights,

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

            Result contextMenuResult = Result.Cancel;

            var items = new List<MenuItem>
            {
                new MenuItem
                {
                    Text = "Add new waypoint",
                    IconImageSource = ImageSource.FromFile("playlist_plus.png"),
                    Command = new AsyncRelayCommand(async () => await CloseWithResult(Result.AddNewWaypoint)),
                },
                new MenuItem
                {
                    Text = "Set as compass target",
                    IconImageSource = ImageSource.FromFile("compass_rose.png"),
                    Command = new AsyncRelayCommand(async () => await CloseWithResult(Result.SetAsCompassTarget)),
                },
                new MenuItem
                {
                    Text = "Navigate here",
                    IconImageSource = ImageSource.FromFile("directions.png"),
                    Command = new AsyncRelayCommand(async () => await CloseWithResult(Result.NavigateHere)),
                },
                new MenuItem
                {
                    Text = "Show flying range",
                    IconImageSource = ImageSource.FromFile("arrow_expand_horizontal.png"),
                    Command = new AsyncRelayCommand(async () => await CloseWithResult(Result.ShowFlyingRange)),
                },
                new MenuItem
                {
                    Text = "Plan tour",
                    IconImageSource = ImageSource.FromFile("map_marker_plus.png"),
                    Command = new AsyncRelayCommand(async () => await CloseWithResult(Result.PlanTour)),
                },
                new MenuItem
                {
                    Text = "Find flights",
                    IconImageSource = ImageSource.FromFile("text_box_search_outline.png"),
                    Command = new AsyncRelayCommand(async () => await CloseWithResult(Result.FindFlights)),
                },
            };

            async Task CloseWithResult(Result selectedResult)
            {
                contextMenuResult = selectedResult;
                await (popupPage?.CloseAsync() ?? Task.CompletedTask);
            }

            var viewModel = new ContextMenuPopupViewModel(
                caption,
                items,
                () => { });

            popupPage = new ContextMenuPopupPage(viewModel);

            IPopupResult popupResult = await UserInterface.MainPage.ShowPopupAsync(popupPage);

            if (popupResult.WasDismissedByTappingOutsideOfPopup)
            {
                return Result.Cancel;
            }

            return contextMenuResult;
        }
    }
}
