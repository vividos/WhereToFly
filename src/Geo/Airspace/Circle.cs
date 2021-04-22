namespace WhereToFly.Geo.Airspace
{
    /// <summary>
    /// A circle geometry with center and radius
    /// </summary>
    public class Circle : Geometry
    {
        /// <summary>
        /// Center coordinates of circle
        /// </summary>
        public Coord Center { get; set; }

        /// <summary>
        /// Radius of circle, in meter
        /// </summary>
        public double Radius { get; set; }
    }
}
