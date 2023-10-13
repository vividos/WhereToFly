namespace WhereToFly.Geo.Airspace
{
    /// <summary>
    /// A circle geometry with center and radius
    /// </summary>
    public class Circle : IGeometry
    {
        /// <summary>
        /// Center coordinates of circle
        /// </summary>
        public Coord Center { get; set; }

        /// <summary>
        /// Radius of circle, in meter
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Creates a new circle object
        /// </summary>
        /// <param name="center">center coordinates of circle</param>
        /// <param name="radius">radius of circle, in meter</param>
        public Circle(Coord center, double radius)
        {
            this.Center = center;
            this.Radius = radius;
        }
    }
}
