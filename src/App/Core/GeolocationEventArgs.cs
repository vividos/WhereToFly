using Plugin.Geolocator.Abstractions;
using System;
using WhereToFly.Shared.Model;

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
        public GeolocationEventArgs(Position position)
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
        public Position Position { get; }
    }
}
