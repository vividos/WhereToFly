namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// Type of a location property stored in the Properties dictionary on a location.
    /// </summary>
    public enum LocationPropertyType
    {
        /// <summary>
        /// The property contains the wind direction, in km/h
        /// </summary>
        WindDirection,

        /// <summary>
        /// Current wind speed, in km/h
        /// </summary>
        WindSpeed,

        /// <summary>
        /// Current gusts wind speed, in km/h
        /// </summary>
        WindSpeedGusts,

        /// <summary>
        /// Date/time in ISO8601 format when the next live update is available for this location
        /// </summary>
        NextLiveUpdate,

        /// <summary>
        /// Direction in degrees, without ° unit, of the viewing direction of a webcam
        /// </summary>
        WebcamViewDirection,

        /// <summary>
        /// The angle of view the webcam can display, in degrees
        /// </summary>
        WebcamViewAngle,

        /// <summary>
        /// Thermal quality value of a thermal hotspot, a number between 0 and 100
        /// </summary>
        ThermalQuality,
    }
}
