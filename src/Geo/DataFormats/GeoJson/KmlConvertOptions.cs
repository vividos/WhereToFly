using SharpKml.Base;
using System;
using System.Collections.Generic;

namespace WhereToFly.Geo.DataFormats.GeoJson
{
    /// <summary>
    /// Options for converting GeoJSON to KML
    /// </summary>
    public class KmlConvertOptions
    {
        /// <summary>
        /// Document name
        /// </summary>
        public string DocumentName { get; set; } = string.Empty;

        /// <summary>
        /// Line width for line strings
        /// </summary>
        public double LineWidth { get; set; } = 2.0;

        /// <summary>
        /// KML color for polygons; default: blue
        /// </summary>
        public Color32 PolygonColor { get; set; } = new Color32(255, 255, 0, 0);

        /// <summary>
        /// Custom function to format names for KML elements; may be null
        /// </summary>
        public Func<Dictionary<string, object>, string> CustomNameFormatter { get; set; } = null;

        /// <summary>
        /// Custom function to format description texts for KML elements; may be null
        /// </summary>
        public Func<Dictionary<string, object>, string> CustomDescriptionFormatter { get; set; } = null;
    }
}
