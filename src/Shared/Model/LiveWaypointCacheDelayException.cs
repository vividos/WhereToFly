using System;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Exception that is thrown when retrieving a live waypoint from cache has been delayed, e.g.
    /// when the live waypoint queue is stalled due to too much requests to one service.
    /// </summary>
    public class LiveWaypointCacheDelayException : Exception
    {
        /// <summary>
        /// Date where next request for a specific live waypoint is most probably available
        /// </summary>
        public DateTimeOffset NextRequestDate { get; }

        /// <summary>
        /// Creates a new exception object
        /// </summary>
        /// <param name="message">additional message to pass</param>
        /// <param name="nextRequestDate">next request date to send to client</param>
        public LiveWaypointCacheDelayException(string message, DateTimeOffset nextRequestDate)
            : base(message)
        {
            this.NextRequestDate = nextRequestDate;
        }
    }
}
