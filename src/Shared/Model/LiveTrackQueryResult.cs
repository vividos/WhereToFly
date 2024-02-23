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
        /// Creates a new live track query result
        /// </summary>
        /// <param name="data">live track data</param>
        /// <param name="nextRequestDate">next request date</param>
        public LiveTrackQueryResult(
            LiveTrackData data,
            DateTimeOffset nextRequestDate)
        {
            this.Data = data;
            this.NextRequestDate = nextRequestDate;
        }

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
