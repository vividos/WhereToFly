using System;
using WhereToFly.Geo.Model;

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
        public GeolocationEventArgs(Xamarin.Essentials.Location position)
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
        public Xamarin.Essentials.Location Position { get; }
    }
}
