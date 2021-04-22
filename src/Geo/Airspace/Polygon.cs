using System.Collections.Generic;

#pragma warning disable CA1034 // Nested types should not be visible

namespace WhereToFly.Geo.Airspace
{
    /// <summary>
    /// Polygon geometry with polygon segments and arcs
    /// </summary>
    public class Polygon : Geometry
    {
        /// <summary>
        /// Direction of arc
        /// </summary>
        public enum ArcDirection
        {
            /// <summary>
            /// Arc revolves clockwise around the center
            /// </summary>
            Clockwise,

            /// <summary>
            /// Arc revolves counter-clockwise around the center
            /// </summary>
            CounterClockwise,
        }

        /// <summary>
        /// Base class for a polygon segment
        /// </summary>
        public class PolygonSegment
        {
        }

        /// <summary>
        /// A point in the polygon
        /// </summary>
        public class PolygonPoint : PolygonSegment
        {
            /// <summary>
            /// Polygon point coordinates
            /// </summary>
            public Coord Point { get; set; }
        }

        /// <summary>
        /// A polygon arc, defined by a center point, a start point and an end point
        /// </summary>
        public class PolygonArc : PolygonSegment
        {
            /// <summary>
            /// Arc center point
            /// </summary>
            public Coord Center { get; set; }

            /// <summary>
            /// Arc start point
            /// </summary>
            public Coord Start { get; set; }

            /// <summary>
            /// Arc end point
            /// </summary>
            public Coord End { get; set; }

            /// <summary>
            /// Arc direction
            /// </summary>
            public ArcDirection Direction { get; set; }
        }

        /// <summary>
        /// A polygon arc segment, defined by center point, radius, start and end angle and an arc
        /// direction
        /// </summary>
        public class PolygonArcSegment : PolygonSegment
        {
            /// <summary>
            /// Arc segment center point
            /// </summary>
            public Coord Center { get; set; }

            /// <summary>
            /// Circle radius, in meter
            /// </summary>
            public double Radius { get; set; }

            /// <summary>
            /// Arc segment start angle, in degrees
            /// </summary>
            public double StartAngle { get; set; }

            /// <summary>
            /// Arc segment end angle, in degrees
            /// </summary>
            public double EndAngle { get; set; }

            /// <summary>
            /// Arc segment direction
            /// </summary>
            public ArcDirection Direction { get; set; }
        }

        /// <summary>
        /// List of all segments in this polygon
        /// </summary>
        public List<PolygonSegment> Segments { get; set; } = new List<PolygonSegment>();
    }
}
