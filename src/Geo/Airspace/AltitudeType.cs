namespace WhereToFly.Geo.Airspace
{
    /// <summary>
    /// Altitude (reference) type
    /// </summary>
    public enum AltitudeType
    {
        /// <summary>
        /// Ground/surface level; height value is ignored
        /// </summary>
        GND,

        /// <summary>
        /// Unlimited altitude; height value is ignored
        /// </summary>
        Unlimited,

        /// <summary>
        /// Altitude is given as text and must be user interpreted
        /// </summary>
        Textual,

        /// <summary>
        /// Feet above mean sea level
        /// </summary>
        AMSL,

        /// <summary>
        /// Feet above ground level; depends on the terrain
        /// </summary>
        AGL,

        /// <summary>
        /// Flight level, e.g. FL100
        /// </summary>
        FlightLevel,
    }
}
