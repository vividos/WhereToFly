namespace WhereToFly.App.Core
{
    /// <summary>
    /// Popup page keys for use in navigation service
    /// </summary>
    public enum PopupPageKey
    {
        /// <summary>
        /// Popup page key to show the "add layer" popup page. The layer to be added must be
        /// passed as parameter to the NavigationService.
        /// </summary>
        AddLayerPopupPage,

        /// <summary>
        /// Popup page key to show the weather icon selection popup page. The group (or null) must
        /// be passed as parameter to the NavigationService. The page returns the selected
        /// WeatherIconDescription object as result.
        /// </summary>
        SelectWeatherIconPopupPage,

        /// <summary>
        /// Popup page key to show a popup to set the compass target direction. The page returns
        /// a Tuple of a single int with the direction, or null when the popup was dismissed.
        /// </summary>
        SetCompassTargetDirectionPopupPage,
    }
}
