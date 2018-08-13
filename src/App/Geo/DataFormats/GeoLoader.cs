using System;
using System.Collections.Generic;
using System.IO;
using WhereToFly.App.Model;

namespace WhereToFly.App.Geo.DataFormats
{
    /// <summary>
    /// Loader class for different geo objects, like location lists
    /// </summary>
    public static class GeoLoader
    {
        /// <summary>
        /// Loads a location list from given filename; must have .kml, .kmz or .gpx extension.
        /// </summary>
        /// <param name="filename">filename of file to load</param>
        /// <returns>list of locations found in the file</returns>
        public static List<Location> LoadLocationList(string filename)
        {
            using (Stream stream = new FileStream(filename, FileMode.Open))
            {
                return LoadLocationList(stream, filename);
            }
        }

        /// <summary>
        /// Loads a location list from given stream
        /// </summary>
        /// <param name="stream">stream of file to load</param>
        /// <param name="filename">
        /// the filename of the stream; this is used to determine the file type, so it can also be
        /// a dummy filename or a relative filename
        /// </param>
        /// <returns>list of locations found in the file</returns>
        public static List<Location> LoadLocationList(Stream stream, string filename)
        {
            string extension = Path.GetExtension(filename);

            switch (extension)
            {
                case ".kml":
                    return KmlFormatLoader.LoadLocationList(stream, isKml: true);

                case ".kmz":
                    return KmlFormatLoader.LoadLocationList(stream, isKml: false);

                case ".gpx":
                    return GpxFormatLoader.LoadLocationList(stream);

                default:
                    throw new ArgumentException("file is not a valid .kml, .kmz or .gpx file");
            }
        }

        /// <summary>
        /// Loads a track from stream with given filename, and track index, in case the file
        /// contains multiple tracks
        /// </summary>
        /// <param name="stream">stream to load from</param>
        /// <param name="filename">filename part of stream</param>
        /// <param name="trackIndex">track index</param>
        /// <returns>loaded track</returns>
        public static Track LoadTrack(Stream stream, string filename, int trackIndex)
        {
            string extension = Path.GetExtension(filename);

            switch (extension)
            {
                case ".igc":
                    return IgcFormatLoader.LoadTrack(stream, trackIndex);

                default:
                    throw new ArgumentException("file is not a valid .igc file");
            }
        }
    }
}
