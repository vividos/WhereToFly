namespace WhereToFly.App.Core
{
    /// <summary>
    /// Page keys for use in navigation service
    /// </summary>
    public enum PageKey
    {
        /// <summary>
        /// Page key to navigate to map page
        /// </summary>
        MapPage,

        /// <summary>
        /// Page key to navigate to layer list page
        /// </summary>
        LayerListPage,

        /// <summary>
        /// Page key to navigate to layer details page. The layer must be passed as parameter to
        /// the NavigationService.
        /// </summary>
        LayerDetailsPage,

        /// <summary>
        /// Page key to navigate to current position details page
        /// </summary>
        CurrentPositionDetailsPage,

        /// <summary>
        /// Page key to navigate to location list page
        /// </summary>
        LocationListPage,

        /// <summary>
        /// Page key to navigate to location details page. The location must be passed as
        /// parameter to the NavigationService.
        /// </summary>
        LocationDetailsPage,

        /// <summary>
        /// Page key to navigate to edit location details page. The location to edit must be
        /// passed as parameter to the NavigationService.
        /// </summary>
        EditLocationDetailsPage,

        /// <summary>
        /// Page key to navigate to track list page.
        /// </summary>
        TrackListPage,

        /// <summary>
        /// Page key to navigate to track info page.
        /// </summary>
        TrackInfoPage,

        /// <summary>
        /// Page key to navigate to track height profile page.
        /// </summary>
        TrackHeightProfilePage,

        /// <summary>
        /// Page key to navigate to weather dashboard page.
        /// </summary>
        WeatherDashboardPage,

        /// <summary>
        /// Page key to navigate to weather details page. The page must be started with a string
        /// parameter specifying the URL of the web page to display.
        /// </summary>
        WeatherDetailsPage,

        /// <summary>
        /// Page key to navigate to settings page
        /// </summary>
        SettingsPage,

        /// <summary>
        /// Page key to navigate to info page
        /// </summary>
        InfoPage,
    }
}
