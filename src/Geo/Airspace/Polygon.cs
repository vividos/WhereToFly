using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable CA1034 // Nested types should not be visible

namespace WhereToFly.Geo.Airspace
{
    /// <summary>
    /// Polygon geometry with polygon segments and arcs
    /// </summary>
    public class Polygon : IGeometry
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
        [DebuggerDisplay("PolygonSegment")]
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

            /// <summary>
            /// Creates a new polygon point
            /// </summary>
            /// <param name="point">polygon point</param>
            public PolygonPoint(Coord point)
            {
                this.Point = point;
            }
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

            /// <summary>
            /// Creates a new polygon arc object
            /// </summary>
            /// <param name="center">arc center point</param>
            /// <param name="start">arc start point</param>
            /// <param name="end">arc end point</param>
            /// <param name="direction">arc direction</param>
            public PolygonArc(Coord center, Coord start, Coord end, ArcDirection direction)
            {
                this.Center = center;
                this.Start = start;
                this.End = end;
                this.Direction = direction;
            }
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
            /// Arc segment radius, in meter
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

            /// <summary>
            /// Creates a new polygon arc segment object
            /// </summary>
            /// <param name="center">arc segment center point</param>
            /// <param name="radius">arc segment radius, in meter</param>
            /// <param name="startAngle">arc segment start angle, in degrees</param>
            /// <param name="endAngle">arc end point</param>
            /// <param name="direction">arc direction</param>
            public PolygonArcSegment(
                Coord center,
                double radius,
                double startAngle,
                double endAngle,
                ArcDirection direction)
            {
                this.Center = center;
                this.Radius = radius;
                this.StartAngle = startAngle;
                this.EndAngle = endAngle;
                this.Direction = direction;
            }
        }

        /// <summary>
        /// List of all segments in this polygon
        /// </summary>
        public List<PolygonSegment> Segments { get; set; } = new List<PolygonSegment>();
    }
}
