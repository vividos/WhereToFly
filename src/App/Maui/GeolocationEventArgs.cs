using WhereToFly.Geo.Model;

namespace WhereToFly.App
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
        public GeolocationEventArgs(Microsoft.Maui.Devices.Sensors.Location position)
        {
            this.Point = position.ToMapPoint();

            this.Position = position;
        }

        /// <summary>
        /// Map point
        /// </summary>
        public MapPoint Point { get; }

        /// <summary>
        /// Position object
        /// </summary>
        public Microsoft.Maui.Devices.Sensors.Location Position { get; }
    }
}
