using System;

namespace WhereToFly.WebApi.Logic
{
    /// <summary>
    /// Contains a live waypoint unique ID
    /// </summary>
    public class LiveWaypointID
    {
        /// <summary>
        /// Application prefix; set before every ID
        /// </summary>
        private const string AppPrefix = "wheretofly";

        /// <summary>
        /// Live Waypoint type
        /// </summary>
        public enum WaypointType
        {
            /// <summary>
            /// Find me SPOT live location; the data part contains the SPOT feed ID
            /// </summary>
            FindMeSpot = 0,
        }

        /// <summary>
        /// Type of the live waypoint
        /// </summary>
        public WaypointType Type { get; set; }

        /// <summary>
        /// Custom data; the meaning is specific for the different waypoint types
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Creates a new live waypoint ID object
        /// </summary>
        /// <param name="type">waypoint type to use</param>
        public LiveWaypointID(WaypointType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Returns a displayable text of the live waypoint ID; this can be used to transfer the
        /// live waypoint ID over the network.
        /// </summary>
        /// <returns>displayable text</returns>
        public override string ToString()
        {
            string type = this.Type.ToString().ToLowerInvariant();

            return $"{AppPrefix}-{type}-{this.Data}";
        }

        /// <summary>
        /// Converts a live waypoint ID in string format to a live waypoint ID object.
        /// </summary>
        /// <param name="id">ID to convert to object</param>
        /// <returns>converted object</returns>
        public static LiveWaypointID FromString(string id)
        {
            var parts = id.Split('-');
            if (parts.Length != 3 ||
                parts[0] != AppPrefix)
            {
                throw new FormatException("invalid Live Waypoint ID");
            }

            return new LiveWaypointID(FindWaypointTypeFromString(parts[1]))
            {
                Data = parts[2],
            };
        }

        /// <summary>
        /// Converts a string representation of a waypoint type to the enum value
        /// </summary>
        /// <param name="waypointType">waypoint type in all lowercase characters</param>
        /// <returns>found waypoint type</returns>
        private static WaypointType FindWaypointTypeFromString(string waypointType)
        {
            foreach (var item in Enum.GetValues(typeof(WaypointType)))
            {
                if (item.ToString().ToLowerInvariant() == waypointType)
                {
                    return (WaypointType)item;
                }
            }

            throw new FormatException("invalid waypoint time in live waypoint ID");
        }
    }
}
