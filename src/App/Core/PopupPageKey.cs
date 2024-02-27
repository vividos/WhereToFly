namespace WhereToFly.App
{
    /// <summary>
    /// Popup page keys for use in navigation service
    /// </summary>
    public enum PopupPageKey
    {
        /// <summary>
        /// Popup page key to show the "add layer" popup page.
        /// Parameter: Layer object to be added
        /// </summary>
        AddLayerPopupPage,

        /// <summary>
        /// Popup page key to show the "add live waypoint" popup page.
        /// Parameter: Location object to be added
        /// Returns: Edited Location object, or null when popup was dismissed
        /// </summary>
        AddLiveWaypointPopupPage,

        /// <summary>
        /// Popup page key to show the "add track" popup page.
        /// Parameter: Track to add
        /// Returns: Track object, or null when popup was dismissed
        /// </summary>
        AddTrackPopupPage,

        /// <summary>
        /// Popup page key to show the "add weather link" popup page.
        /// Returns: WeatherIconDescription object, or null when popup was dismissed
        /// </summary>
        AddWeatherLinkPopupPage,

        /// <summary>
        /// Popup page key to show the pop page to specifying a filter based on takeoff.
        /// directions.
        /// Parameter: LocationFilterSettings object to edit
        /// Returns: LocationFilterSettings object, or null when popup was dismissed
        /// </summary>
        FilterTakeoffDirectionsPopupPage,

        /// <summary>
        /// Popup page key to show the "find location" pop page.
        /// Returns: string with entered text, or null when popup was dismissed
        /// </summary>
        FindLocationPopupPage,

        /// <summary>
        /// Popup page key to show popup page for setting parameters to show flying range.
        /// Returns: FlyingRangeParameters object, or null when popup was dismissed
        /// </summary>
        FlyingRangePopupPage,

        /// <summary>
        /// Popup page key to show the popup to show all locations selected for tour planning, and
        /// to let user start planning tour.
        /// Parameter: PlanTourParameters
        /// Returns: Task to wait on
        /// </summary>
        PlanTourPopupPage,

        /// <summary>
        /// Popup page key to show the popup for selecting airspace classes to import.
        /// Parameter: IEnumerable of AirspaceClass
        /// Returns: ISet of AirspaceClass, or null when popup was dismissed
        /// </summary>
        SelectAirspaceClassPopupPage,

        /// <summary>
        /// Popup page key to show the weather icon selection popup page.
        /// Parameter: The weather icon group as string (or null)
        /// Returns: WeatherIconDescription object, or null when the popup was dismissed
        /// </summary>
        SelectWeatherIconPopupPage,

        /// <summary>
        /// Popup page key to show a popup to set the compass target direction.
        /// Returns: Tuple of a single int with the direction, or null when the popup was dismissed
        /// </summary>
        SetCompassTargetDirectionPopupPage,

        /// <summary>
        /// Popup page key to show a popup to set track infos.
        /// Parameter: Track object to edit
        /// Returns: Track object, or null when popup was dismissed
        /// </summary>
        SetTrackInfosPopupPage,
    }
}
