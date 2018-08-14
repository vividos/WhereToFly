using System.Collections.Generic;
using WhereToFly.App.Geo;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Equality comparer implementation for Track.
    /// </summary>
    internal class TrackEqualityComparer : IEqualityComparer<Track>
    {
        /// <summary>
        /// Compares two tracks for equality
        /// </summary>
        /// <param name="x">first track to compare</param>
        /// <param name="y">second track to compare to first</param>
        /// <returns>true when track are equal, false when not</returns>
        public bool Equals(Track x, Track y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Id == y.Id &&
                x.Name == y.Name &&
                x.TrackPoints.Count == y.TrackPoints.Count;
        }

        /// <summary>
        /// Returns hash code for given track
        /// </summary>
        /// <param name="obj">track object</param>
        /// <returns>calculated hash code</returns>
        public int GetHashCode(Track obj)
        {
            return obj.GetHashCode();
        }
    }
}
