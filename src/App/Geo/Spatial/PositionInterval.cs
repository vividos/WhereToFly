using System;
using System.Collections.Generic;
using System.Linq;

namespace WhereToFly.App.Geo.Spatial
{
    /// <summary>
    /// An interval of positions, limited by the interval time. Only positions in the specified
    /// interval are kept in the position list.
    /// </summary>
    public class PositionInterval
    {
        /// <summary>
        /// Interval length
        /// </summary>
        private readonly TimeSpan intervalLength;

        /// <summary>
        /// List of all positions in the interval
        /// </summary>
        public LinkedList<Position> PositionList { get; private set; } = new LinkedList<Position>();

        /// <summary>
        /// Creates a new position interval
        /// </summary>
        /// <param name="intervalLength">length of the interval used for limiting position entries</param>
        public PositionInterval(TimeSpan intervalLength)
        {
            this.intervalLength = intervalLength;
        }

        /// <summary>
        /// Adds new position and adjusts position list accordingly
        /// </summary>
        /// <param name="position">position to add</param>
        public void Add(Position position)
        {
            this.PositionList.AddLast(position);

            DateTimeOffset newLimit = position.Timestamp - this.intervalLength;

            // remove all positions before the timestam
            while (this.PositionList.Any() &&
                this.PositionList.First().Timestamp < newLimit)
            {
                this.PositionList.RemoveFirst();
            }
        }
    }
}
