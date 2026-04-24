using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using WhereToFly.Geo.DataFormats.Czml;
using WhereToFly.Geo.Model;
using WhereToFly.Geo.Serializers;

namespace WhereToFly.Geo.DataFormats;

/// <summary>
/// Parses .xctsk competition task files from XContest/XCTrack
/// https://xctrack.org/Competition_Interfaces.html
/// </summary>
public class XcTskFileParser
{
    /// <summary>
    /// Color for start of speed section cylinder
    /// </summary>
    private const string StartOfSpeedSectionColor = "00ff00"; // green

    /// <summary>
    /// Color for default turnpoint cylinder
    /// </summary>
    private const string DefaultTurnpointColor = "0000ff"; // blue

    /// <summary>
    /// Color for end of speed section
    /// </summary>
    private const string EndOfSpeedSectionColor = "ff0000"; // red

    /// <summary>
    /// Filename of file being parsed
    /// </summary>
    private readonly string filename;

    /// <summary>
    /// Parsed JSON document
    /// </summary>
    private readonly XcTskDocument document;

    /// <summary>
    /// Description of task file, in MarkDown format
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new parser object and parses the JSON document
    /// </summary>
    /// <param name="filename">filename of file to parse</param>
    /// <param name="stream">stream to parse</param>
    /// <exception cref="FormatException">
    /// thrown when the JSON format didn't match
    /// </exception>
    public XcTskFileParser(string filename, Stream stream)
    {
        this.filename = filename;

        this.document = JsonSerializer.Deserialize(
            stream,
            XcTskObjectModelJsonSerializerContext.Default.XcTskDocument)
            ?? throw new FormatException("couldn't parse .xctsk file");

        this.Description = this.GenerateDescriptionText();
    }

    /// <summary>
    /// Generates a description text based on all turnpoints
    /// </summary>
    /// <returns>description text</returns>
    private string GenerateDescriptionText()
    {
        var sb = new StringBuilder();

        sb.AppendLine("| Waypoints | Radius |");
        sb.AppendLine("| --------- | ------ |");

        bool isAfterEss = false;
        int waypointIndex = 1;

        foreach (Turnpoint turnpoint in this.document.Turnpoints)
        {
            string turnpointPrefix = turnpoint.Type;
            if (turnpointPrefix == string.Empty)
            {
                turnpointPrefix = isAfterEss
                    ? "GOAL"
                    : $"WP{waypointIndex++}";
            }

            string turnpointText = turnpoint.Waypoint.Name;

            if (!string.IsNullOrWhiteSpace(turnpoint.Waypoint.Description))
            {
                turnpointText += $" {turnpoint.Waypoint.Description}";
            }

            turnpointText += $" {turnpoint.Waypoint.Altitude} m";

            sb.AppendLine($"| {turnpointPrefix} {turnpointText} | {turnpoint.Radius / 1000.0:F1} km |");

            if (!isAfterEss && turnpoint.Type == "ESS")
            {
                isAfterEss = true;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts the loaded and parsed task to CZML
    /// </summary>
    /// <returns>CZML JSON string</returns>
    public string ConvertToCzml()
    {
        var objectList = new List<CzmlBase>
        {
            new PacketHeader(this.filename, this.Description),
        };

        foreach (Turnpoint turnpoint in this.document.Turnpoints)
        {
            var czmlObject = CzmlObjectFromTurnpoint(turnpoint);

            objectList.Add(czmlObject);
        }

        return Serializer.ToCzml(objectList);
    }

    /// <summary>
    /// Creates a CZML object from given turnpoint
    /// </summary>
    /// <param name="turnpoint">turnpoint to use</param>
    /// <returns>CZML object</returns>
    private static Czml.Object CzmlObjectFromTurnpoint(Turnpoint turnpoint)
    {
        var positions = new Czml.PositionList();

        var center = new MapPoint(
            turnpoint.Waypoint.Latitude,
            turnpoint.Waypoint.Longitude);

        const double step = 5;
        const double endAngle = 360.0 + step;

        for (double bearing = 0.0; bearing < endAngle; bearing += step)
        {
            var point = center.PolarOffset(turnpoint.Radius, bearing, 0.0);
            positions.Add(point.Latitude, point.Longitude, 0.0);
        }

        return new Czml.Object
        {
            Name = turnpoint.Type,
            Description = DescriptionFromTurnpoint(turnpoint),
            Polyline = new Polyline
            {
                Positions = positions,
                ClampToGround = true,
                Width = 5.0,
                Material = Material.FromSolidColor(ColorFromTurnpoint(turnpoint, outlineColor: false)),
            },
            Polygon = new Polygon
            {
                Positions = positions,
                Height = 0.0,
                ExtrudedHeight = 3000.0,
                HeightReference = HeightReference.ClampToGround,
                Fill = false,
                Outline = true,
                OutlineColor = ColorFromTurnpoint(turnpoint, outlineColor: true),
                OutlineWidth = 10.0,
                CloseTop = false,
                CloseBottom = false,
            },
        };
    }

    /// <summary>
    /// Returns a formatted description for given turnpoint
    /// </summary>
    /// <param name="turnpoint">turnpoint to use</param>
    /// <returns>description text</returns>
    private static string DescriptionFromTurnpoint(Turnpoint turnpoint)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{turnpoint.Type}: {turnpoint.Waypoint.Name}");
        sb.AppendLine("Radius: " + turnpoint.Radius + " m");
        sb.AppendLine($"Altitude: {turnpoint.Waypoint.Altitude} m");
        sb.AppendLine(turnpoint.Waypoint.Description);

        return sb.ToString();
    }

    /// <summary>
    /// Calculates a color from given turnpoint; uses a default color when none was set.
    /// Outline colors are non-transparent and 20% darker, normal color is half transparent.
    /// </summary>
    /// <param name="turnpoint">turnpoint to use</param>
    /// <param name="outlineColor">true when outline color should be generated</param>
    /// <returns>color object</returns>
    private static Color ColorFromTurnpoint(Turnpoint turnpoint, bool outlineColor)
    {
        string color = turnpoint.Type switch
        {
            "SSS" => StartOfSpeedSectionColor,
            "ESS" => EndOfSpeedSectionColor,
            _ => DefaultTurnpointColor,
        };

        if (!int.TryParse(
            color,
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture,
            out int rgba))
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

        return new Color(r, g, b, outlineColor ? 255 : 128);
    }
}
