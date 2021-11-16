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
    }
}
