using System.Collections.Generic;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.DataFormats
{
    /// <summary>
    /// Interface to a geo data file that contains tracks and/or locations
    /// </summary>
    public interface IGeoDataFile
    {
        /// <summary>
        /// Returns if the opened file contains any locations
        /// </summary>
        /// <returns>true when the file contains locations, false when not</returns>
        bool HasLocations();

        /// <summary>
        /// Returns list of tracks contained in the file
        /// </summary>
        /// <returns>list of track descriptions</returns>
        List<string> GetTrackList();

        /// <summary>
        /// Loads track with given index
        /// </summary>
        /// <param name="trackIndex">track index to load</param>
        /// <returns>loaded track</returns>
        Track LoadTrack(int trackIndex);

        /// <summary>
        /// Loads location list, containing all waypoints in the file
        /// </summary>
        /// <returns>list of locations</returns>
        List<Location> LoadLocationList();
    }
}
