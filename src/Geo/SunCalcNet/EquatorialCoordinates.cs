namespace WhereToFly.Geo.SunCalcNet
{
    /// <summary>
    /// Equatorial coordinates, see:
    /// https://en.wikipedia.org/wiki/Equatorial_coordinate_system
    /// </summary>
    public record EquatorialCoordinates
    {
        /// <summary>
        /// Decliation value, in radians
        /// </summary>
        public double Declination { get; set; }

        /// <summary>
        /// Right ascension value, in radians
        /// </summary>
        public double RightAscension { get; set; }
    }
}
