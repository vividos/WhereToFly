using System;
using System.Collections.Generic;
using System.IO;
using WhereToFly.Geo.Model;

// make Geo internals visible to unit tests
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WhereToFly.App.UnitTest")]

namespace WhereToFly.Geo.DataFormats
{
    /// <summary>
    /// Data file for loading IGC flight log data format
    /// </summary>
    internal class IgcDataFile : IGeoDataFile
    {
        /// <summary>
        /// Stream of IGC data file
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// Creates new IGC data file object
        /// </summary>
        /// <param name="stream">stream to read from</param>
        public IgcDataFile(Stream stream)
        {
            this.stream = new MemoryStream();
            stream.CopyTo(this.stream);
            this.stream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Returns if the opened file contains any locations
        /// </summary>
        /// <returns>true when the file contains locations, false when not</returns>
        public bool HasLocations()
        {
            return false;
        }

        /// <summary>
        /// Returns list of tracks contained in the file; always returns one entry.
        /// </summary>
        /// <returns>list of track descriptions</returns>
        public List<string> GetTrackList()
        {
            return new List<string> { "IGC Track" };
        }

        /// <summary>
        /// Loads track with given index
        /// </summary>
        /// <param name="trackIndex">track index to load</param>
        /// <returns>loaded track</returns>
        public Track LoadTrack(int trackIndex)
        {
            var parser = new IgcParser(this.stream);

            return parser.Track;
        }

        /// <summary>
        /// Loads location list, containing all waypoints in the file
        /// </summary>
        /// <returns>list of locations</returns>
        public List<Location> LoadLocationList()
        {
            throw new NotImplementedException();
        }
    }
}
