using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using WhereToFly.Geo.Airspace;

namespace WhereToFly.Geo.DataFormats
{
    /// <summary>
    /// CZML Writer for airspaces
    /// </summary>
    public static class CzmlAirspaceWriter
    {
        /// <summary>
        /// Value to use when an Unlimited Altitude object should be displayed; in meter
        /// </summary>
        private const double UnlimitedAltitudeHeightInMeter = 10000.0;

        /// <summary>
        /// Default airspace color
        /// </summary>
        private const string DefaultAirspaceColor = "C0C0C0";

        /// <summary>
        /// Writes a list of airspaces to a .czml text string
        /// </summary>
        /// <param name="name">name of czml document</param>
        /// <param name="allAirspaces">enumerable of airspaces</param>
        /// <param name="descriptionLines">list of description lines; may be empty</param>
        /// <returns>valid .czml file content</returns>
        public static string WriteCzml(string name, IEnumerable<Airspace.Airspace> allAirspaces, IEnumerable<string> descriptionLines)
        {
            string description = string.Join("\n", descriptionLines).Trim();
            description = description.Replace("\n", "<br/>");

            if (description.Length == 0)
            {
                description = null;
            }

            var objectList = new List<object>
            {
                new Czml.PacketHeader(name, description),
            };

            foreach (var airspace in allAirspaces)
            {
                var czmlObject = CzmlObjectFromAirspace(airspace);

                if (czmlObject == null)
                {
                    continue;
                }

                objectList.Add(czmlObject);
            }

            return JsonConvert.SerializeObject(objectList);
        }

        /// <summary>
        /// Creates a Czml.Object from given airspace
        /// </summary>
        /// <param name="airspace">airspace to use</param>
        /// <returns>created CZML object</returns>
        private static Czml.Object CzmlObjectFromAirspace(Airspace.Airspace airspace)
        {
            double height = HeightFromAltitude(airspace.Ceiling) - HeightFromAltitude(airspace.Floor);

            if (airspace.Geometry is Circle circle)
            {
                return new Czml.Object
                {
                    Name = airspace.Name,
                    Description = DescriptionFromAirspace(airspace),
                    Position = PositionFromCoord(circle.Center, airspace.Floor),
                    Cylinder = new Czml.Cylinder
                    {
                        BottomRadius = circle.Radius,
                        TopRadius = circle.Radius,
                        Length = height,
                        HeightReference = HeightReferenceFromAltitude(airspace.Floor),
                        Material = MaterialFromAirspace(airspace),
                        Outline = true,
                        OutlineColor = ColorFromAirspace(airspace, outlineColor: true),
                    },
                };
            }

            if (airspace.Geometry is Airspace.Polygon polygon)
            {
                Czml.PositionList positions = GetPolygonPointsFromAirspacePolygon(polygon, airspace.Floor);

                if (!positions.CartographicDegrees.Any())
                {
                    return null;
                }

                return new Czml.Object
                {
                    Name = airspace.Name,
                    Description = DescriptionFromAirspace(airspace),
                    Polygon = new Czml.Polygon
                    {
                        Positions = positions,
                        Height = HeightFromAltitude(airspace.Floor),
                        ExtrudedHeight = HeightFromAltitude(airspace.Ceiling),
                        HeightReference = HeightReferenceFromAltitude(airspace.Floor),
                        Material = MaterialFromAirspace(airspace),
                        Outline = true,
                        OutlineColor = ColorFromAirspace(airspace, outlineColor: true),
                    },
                };
            }

            throw new FormatException("invalid airspace geometry");
        }

        /// <summary>
        /// Returns all polygon points from given airspace polygon object
        /// </summary>
        /// <param name="polygon">airspace polygon</param>
        /// <param name="floor">floor altitude</param>
        /// <returns>position list object with all polygon points</returns>
        private static Czml.PositionList GetPolygonPointsFromAirspacePolygon(Airspace.Polygon polygon, Altitude floor)
        {
            double height = HeightFromAltitude(floor);

            var positions = new Czml.PositionList();

            foreach (var segment in polygon.Segments)
            {
                if (segment is Airspace.Polygon.PolygonPoint point)
                {
                    positions.Add(point.Point.Latitude, point.Point.Longitude, height);
                }

                if (segment is Airspace.Polygon.PolygonArc arc)
                {
                    var convertedArcSegment = ConvertPolygonArcToArcSegment(arc, height);
                    AddArcSegmentPolygonPoints(convertedArcSegment, positions, height);

                    positions.Add(arc.End.Latitude, arc.End.Longitude, height);
                }

                if (segment is Airspace.Polygon.PolygonArcSegment arcSegment)
                {
                    AddArcSegmentPolygonPoints(arcSegment, positions, height);
                }
            }

            return positions;
        }

        /// <summary>
        /// Converts a PolygonArc object to a PolygonArcSegment object and returns it. This is
        /// done by getting the arc segment radius from the distance center to start, the start
        /// and end angles by calculating the course from center to start end end point. When the
        /// distance from center to end point doesn't match the radius (the end point doesn't lie
        /// on the circle) the caller has to add the end point to the polygon, too.
        /// </summary>
        /// <param name="arc">arc object</param>
        /// <param name="height">height of arc</param>
        /// <returns>arg segment object</returns>
        private static Airspace.Polygon.PolygonArcSegment ConvertPolygonArcToArcSegment(
            Airspace.Polygon.PolygonArc arc,
            double height)
        {
            var start = arc.Start.ToMapPoint(height);
            var end = arc.End.ToMapPoint(height);
            var center = arc.Center.ToMapPoint(height);

            return new Airspace.Polygon.PolygonArcSegment
            {
                Center = arc.Center,
                Direction = arc.Direction,
                Radius = start.DistanceTo(arc.Center.ToMapPoint(height)),
                StartAngle = center.CourseTo(start),
                EndAngle = arc.Center.ToMapPoint(height).CourseTo(end),
            };
        }

        /// <summary>
        /// Generates all points for a PolygonArcSegment object and adds it to the positions
        /// </summary>
        /// <param name="arcSegment">arc segment to use</param>
        /// <param name="positions">positions to add to</param>
        /// <param name="height">height value</param>
        private static void AddArcSegmentPolygonPoints(
            Airspace.Polygon.PolygonArcSegment arcSegment,
            Czml.PositionList positions,
            double height)
        {
            var center = arcSegment.Center.ToMapPoint(height);

            bool isClockwise = arcSegment.Direction == Airspace.Polygon.ArcDirection.Clockwise;

            double endAngle = arcSegment.EndAngle;
            double startAngle = arcSegment.StartAngle;

            // adjust angles so that we don't have to deal with wrap around
            if (isClockwise && startAngle > endAngle)
            {
                endAngle += 360.0;
            }

            if (!isClockwise && startAngle < endAngle)
            {
                startAngle += 360.0;
            }

            bool isAtEnd;
            double step = (isClockwise ? 1.0 : -1.0) * ArcSegmentAngleStepByRadius(arcSegment.Radius);
            double bearing = startAngle;

            do
            {
                var point = center.PolarOffset(arcSegment.Radius, bearing, height);
                positions.Add(point.Latitude, point.Longitude, height);

                bearing += step;
                isAtEnd = isClockwise
                    ? bearing > endAngle
                    : bearing < endAngle;
            }
            while (!isAtEnd);

            // add end point when the step angle didn't hit the end angle directly
            if (Math.Abs(bearing - arcSegment.EndAngle) > 1e-6)
            {
                var point = center.PolarOffset(arcSegment.Radius, arcSegment.EndAngle, height);
                positions.Add(point.Latitude, point.Longitude, height);
            }
        }

        /// <summary>
        /// Depending on the size of the circle, use different number of angles for each arc
        /// segment line, to get smoother arc for larger circles.
        /// </summary>
        /// <param name="radius">arc radius, in meter</param>
        /// <returns>angle of a single line</returns>
        private static double ArcSegmentAngleStepByRadius(double radius)
        {
            if (radius < 10000.0)
            {
                return 5.0;
            }

            if (radius < 25000.0)
            {
                return 3.0;
            }

            if (radius < 50000.0)
            {
                return 2.0;
            }

            return 1.0;
        }

        /// <summary>
        /// Returns a formatted description for given airspace
        /// </summary>
        /// <param name="airspace">airspace to use</param>
        /// <returns>description text</returns>
        private static string DescriptionFromAirspace(Airspace.Airspace airspace)
        {
            var sb = new StringBuilder();

            sb.Append("Name: " + airspace.Name + "\n");
            sb.Append("Class: " + airspace.Class.ToString());
            if (!string.IsNullOrEmpty(airspace.AirspaceType))
            {
                sb.Append($" ({airspace.AirspaceType})");
            }

            sb.Append("\n");

            sb.Append($"{airspace.Floor} -> {airspace.Ceiling}\n");

            if (!string.IsNullOrEmpty(airspace.Description))
            {
                sb.Append($"Description: {airspace.Description}\n");
            }

            if (!string.IsNullOrEmpty(airspace.Frequency))
            {
                sb.Append($"Frequency: {airspace.Frequency}");
            }

            if (!string.IsNullOrEmpty(airspace.CallSign))
            {
                sb.Append($"Call sign: {airspace.CallSign}");
            }

            sb.Replace("\n\n", "\n");
            sb.Replace("\n", "<br/>");

            return sb.ToString();
        }

        /// <summary>
        /// Returns a position from given coordinates and altitude
        /// </summary>
        /// <param name="coord">coordinates object</param>
        /// <param name="altitude">altitude object</param>
        /// <returns>CZML position object</returns>
        private static Czml.PositionList PositionFromCoord(Coord coord, Altitude altitude)
        {
            return new Czml.PositionList
            {
                CartographicDegrees = new List<double>
                {
                    // note the reversal of latitude and longitude
                    coord.Longitude,
                    coord.Latitude,
                    HeightFromAltitude(altitude),
                },
            };
        }

        /// <summary>
        /// Returns height from altitude object
        /// </summary>
        /// <param name="altitude">altitude object</param>
        /// <returns>height value</returns>
        private static double HeightFromAltitude(Altitude altitude)
        {
            switch (altitude.Type)
            {
                case AltitudeType.GND:
                    return 0.0;
                case AltitudeType.Unlimited:
                    return UnlimitedAltitudeHeightInMeter;
                case AltitudeType.Textual:
                    return UnlimitedAltitudeHeightInMeter;
                case AltitudeType.AMSL:
                case AltitudeType.AGL:
                    return altitude.Value * Constants.FactorFeetToMeter;
                case AltitudeType.FlightLevel:
                    return altitude.Value * 100.0 * Constants.FactorFeetToMeter;
                default:
                    Debug.Assert(false, "invalid altitude type");
                    break;
            }

            return 0.0;
        }

        /// <summary>
        /// Returns a height reference for the given altitude
        /// </summary>
        /// <param name="altitude">altitude object</param>
        /// <returns>height reference value</returns>
        private static Czml.HeightReference HeightReferenceFromAltitude(Altitude altitude)
        {
            return altitude.Type switch
            {
                AltitudeType.GND => Czml.HeightReference.ClampToGround,
                AltitudeType.Unlimited or
                AltitudeType.Textual or
                AltitudeType.AGL => Czml.HeightReference.RelativeToGround,
                _ => Czml.HeightReference.None,
            };
        }

        /// <summary>
        /// Returns material object from given airspace
        /// </summary>
        /// <param name="airspace">airspace to use</param>
        /// <returns>material object</returns>
        private static Czml.Material MaterialFromAirspace(Airspace.Airspace airspace)
        {
            var color = ColorFromAirspace(airspace, outlineColor: false);
            return Czml.Material.FromSolidColor(color);
        }

        /// <summary>
        /// Calculates a color from given airspace; uses a default color when none was set.
        /// Outline colors are non-transparent and 20% darker, normal color is half transparent.
        /// </summary>
        /// <param name="airspace">airspace to use</param>
        /// <param name="outlineColor">true when outline color should be generated</param>
        /// <returns>color object</returns>
        private static Czml.Color ColorFromAirspace(Airspace.Airspace airspace, bool outlineColor)
        {
            string color = airspace.Color ?? DefaultAirspaceColor;

            if (!int.TryParse(color, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgba))
            {
                rgba = 0xc0c0c0;
            }

            int r = (rgba & 0xff0000) >> 16;
            int g = (rgba & 0xff00) >> 8;
            int b = rgba & 0xff;

            if (outlineColor)
            {
                r -= (int)(r * 0.2);
                g -= (int)(g * 0.2);
                b -= (int)(b * 0.2);
            }

            return new Czml.Color(r, g, b, outlineColor ? 255 : 128);
        }
    }
}
