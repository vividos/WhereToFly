namespace WhereToFly.Geo
{
    /// <summary>
    /// Geospatial constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Earth radius, in meter (accepted by WGS84 standard)
        /// </summary>
        public static readonly double EarthRadiusInMeter = 6378137;

        /// <summary>
        /// Factor to convert from m/s to km/h
        /// </summary>
        public static readonly double FactorMeterPerSecondToKilometerPerHour = 3.6;

        /// <summary>
        /// Factor to convert feet to meter
        /// </summary>
        public static readonly double FactorFeetToMeter = 0.3048;

        /// <summary>
        /// Factor to convert nautical miles (nm) to meter
        /// </summary>
        public static readonly double FactorNauticalMilesToMeter = 1852.0;
    }
}
