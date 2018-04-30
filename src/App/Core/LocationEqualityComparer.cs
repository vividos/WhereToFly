using System;
using System.Collections.Generic;
using WhereToFly.App.Logic.Model;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Equality comparer implementation for Location. Use this class instead of implementing
    /// Loation.Equals(), since Newtonsoft.Json crashes on Android when deserializing a list of
    /// Location objects on Android.
    /// </summary>
    internal class LocationEqualityComparer : IEqualityComparer<Location>
    {
        /// <summary>
        /// Compares two locations for equality
        /// </summary>
        /// <param name="x">first location to compare</param>
        /// <param name="y">second location to compare to first</param>
        /// <returns></returns>
        public bool Equals(Location x, Location y)
        {
            return x.Id == y.Id &&
                x.Name == y.Name &&
                x.Type == y.Type &&
                x.InternetLink == y.InternetLink &&
                x.MapLocation.Equals(y.MapLocation) &&
                Math.Abs(x.Elevation - y.Elevation) < 1e-6 &&
                x.Description == y.Description;
        }

        /// <summary>
        /// Returns hash code for given location
        /// </summary>
        /// <param name="obj">location object</param>
        /// <returns>calculated hash code</returns>
        public int GetHashCode(Location obj)
        {
            return obj.GetHashCode();
        }
    }
}
