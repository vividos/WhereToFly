using System;
using WhereToFly.Shared.Model;
using Xamarin.Essentials;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Event args for position updates
    /// </summary>
    public class GeolocationEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new event args object
        /// </summary>
        /// <param name="position">position object</param>
        public GeolocationEventArgs(Location position)
        {
            this.Point = new MapPoint(
                position.Latitude,
                position.Longitude,
                position.Altitude);

            this.Position = position;
        }

        /// <summary>
        /// Map point
        /// </summary>
        public MapPoint Point { get; }

        /// <summary>
        /// Position object
        /// </summary>
        public Location Position { get; }
    }
}
