using System.Collections.Generic;
using System.IO;

// make Geo internals visible to unit tests
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WhereToFly.App.UnitTest")]

namespace WhereToFly.App.Geo.DataFormats
{
    /// <summary>
    /// Loader for the IGC flight log data format
    /// </summary>
    internal static class IgcFormatLoader
    {
        /// <summary>
        /// Returns list of tracks contained in the file; always returns one entry.
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <returns>list of track descriptions</returns>
        public static List<string> GetTrackList(Stream stream)
        {
            return new List<string> { "IGC Track" };
        }

        /// <summary>
        /// Loads track from given stream containing an IGC file
        /// </summary>
        /// <param name="stream">stream with IGC data</param>
        /// <param name="trackIndex">track index to load</param>
        /// <returns>loaded track</returns>
        public static Track LoadTrack(Stream stream, int trackIndex)
        {
            var parser = new IgcParser(stream);

            return parser.Track;
        }
    }
}
