using System;

namespace WhereToFly.Shared.Model
{
    /// <summary>
    /// Query result for live track data
    /// </summary>
    [Serializable]
    public class LiveTrackQueryResult
    {
        /// <summary>
        /// Live track data; always set, but may contain zero live track points
        /// </summary>
        public LiveTrackData Data { get; set; }

        /// <summary>
        /// Date where next request for the requested live track will be most probably
        /// available
        /// </summary>
        public DateTimeOffset NextRequestDate { get; set; }
    }
}
