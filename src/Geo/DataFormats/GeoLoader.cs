using System;
using System.IO;

namespace WhereToFly.Geo.DataFormats
{
    /// <summary>
    /// Loader class for different geo objects, like location lists
    /// </summary>
    public static class GeoLoader
    {
        /// <summary>
        /// Opens a geo data file from given filename; must have .kml, .kmz, .gpx, .igc or .cup
        /// extension.
        /// </summary>
        /// <param name="filename">filename of file to load</param>
        /// <returns>geo data file object</returns>
        public static IGeoDataFile LoadGeoDataFile(string filename)
        {
            using Stream stream = new FileStream(filename, FileMode.Open);
            return LoadGeoDataFile(stream, filename);
        }

        /// <summary>
        /// Opens a geo data file from given stream; file name must have .kml, .kmz, .gpx, .igc or
        /// .cup extension.
        /// </summary>
        /// <param name="stream">stream to load</param>
        /// <param name="filename">filename of file to load</param>
        /// <returns>geo data file object</returns>
        public static IGeoDataFile LoadGeoDataFile(Stream stream, string filename)
        {
            string extension = Path.GetExtension(filename);

            return extension.ToLowerInvariant() switch
            {
                ".kml" => new KmlDataFile(stream, filename, isKml: true),
                ".kmz" => new KmlDataFile(stream, filename, isKml: false),
                ".gpx" => new GpxDataFile(stream),
                ".igc" => new IgcDataFile(stream),
                ".cup" => new SeeYouDataFile(stream),
                _ => throw new ArgumentException("file is not a valid .kml, .kmz, .gpx, .igc or .cup file"),
            };
        }
    }
}
