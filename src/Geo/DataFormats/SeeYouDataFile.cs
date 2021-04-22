using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.DataFormats
{
    /// <summary>
    /// Data file for loading locations from SeeYou .cup files.
    /// </summary>
    internal class SeeYouDataFile : IGeoDataFile
    {
        /// <summary>
        /// List of loaded locations
        /// </summary>
        private readonly IEnumerable<Location> locationList;

        /// <summary>
        /// Creates a new SeeYou data file from given stream
        /// </summary>
        /// <param name="stream">stream to use</param>
        public SeeYouDataFile(Stream stream)
        {
            var parser = new SeeYouWaypointsFileParser(stream);

            this.locationList = parser.Parse().ToList();
        }

        /// <summary>
        /// Returns if the opened file contains any locations
        /// </summary>
        /// <returns>true when the file contains locations, false when not</returns>
        public bool HasLocations()
        {
            return this.locationList.Any();
        }

        /// <summary>
        /// Returns location list from .cup file
        /// </summary>
        /// <returns>location list</returns>
        public List<Location> LoadLocationList()
        {
            return this.locationList.ToList();
        }

        /// <summary>
        /// Returns list of tracks contained in the file; always returns an empty list.
        /// </summary>
        /// <returns>list of track descriptions</returns>
        public List<string> GetTrackList()
        {
            return new List<string>();
        }

        /// <summary>
        /// Loads a track; always throws exception, as there are no tracks.
        /// </summary>
        /// <param name="trackIndex">track index; ignored</param>
        /// <returns>throws an exception</returns>
        public Track LoadTrack(int trackIndex)
        {
            throw new NotImplementedException();
        }
    }
}
