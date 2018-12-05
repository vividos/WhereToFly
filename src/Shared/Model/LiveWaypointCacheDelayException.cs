using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Exception that is thrown when retrieving a live waypoint from cache has been delayed, e.g.
    /// when the live waypoint queue is stalled due to too much requests to one service.
    /// </summary>
    [Serializable]
    public sealed class LiveWaypointCacheDelayException : Exception
    {
        /// <summary>
        /// Date where next request for a specific live waypoint is most probably available
        /// </summary>
        public DateTimeOffset NextRequestDate { get; }

        /// <summary>
        /// Creates a new exception object with a default next request date
        /// </summary>
        public LiveWaypointCacheDelayException()
        {
            this.NextRequestDate = DateTimeOffset.MinValue;
        }

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

        /// <summary>
        /// Creates a new exception object with an inner exception
        /// </summary>
        /// <param name="message">additional message to pass</param>
        /// <param name="nextRequestDate">next request date to send to client</param>
        /// <param name="innerException">inner exception</param>
        public LiveWaypointCacheDelayException(string message, DateTimeOffset nextRequestDate, Exception innerException)
            : base(message, innerException)
        {
            this.NextRequestDate = nextRequestDate;
        }

        /// <summary>
        /// Creates a new exception object with an inner exception and a default next request date
        /// </summary>
        /// <param name="message">additional message to pass</param>
        /// <param name="innerException">inner exception</param>
        public LiveWaypointCacheDelayException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.NextRequestDate = DateTimeOffset.MinValue;
        }

        /// <summary>
        /// Deserialisation ctor
        /// </summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">streaming context</param>
        private LiveWaypointCacheDelayException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.NextRequestDate = new DateTimeOffset(info.GetDateTime(nameof(this.NextRequestDate)));
        }

        /// <summary>
        /// Serialisation method
        /// </summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">streaming context</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(this.NextRequestDate), this.NextRequestDate);

            // MUST call through to the base class to let it save its own state
            base.GetObjectData(info, context);
        }
    }
}
