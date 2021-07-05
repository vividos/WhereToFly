using Refit;
using System;
using System.Threading.Tasks;
using WhereToFly.Shared.Model;

namespace WhereToFly.Web.LiveTracking
{
    /// <summary>
    /// Web API backend interface
    /// </summary>
    internal interface IBackendWebApi
    {
        /// <summary>
        /// Retrieves latest info about a live waypoint, including new coordinates and
        /// description.
        /// </summary>
        /// <param name="liveWaypointId">live waypoint ID</param>
        /// <returns>query result for live waypoint</returns>
        [Get("/api/LiveWaypoint?id={id}")]
        Task<LiveWaypointQueryResult> GetLiveWaypointDataAsync([AliasAs("id")] string liveWaypointId);

        /// <summary>
        /// Retrieves latest info about a live track, including new coordinates and
        /// description.
        /// </summary>
        /// <param name="liveTrackId">live track ID</param>
        /// <param name="lastTrackPointTime">
        /// last track point that the client already has received, or null when no track points
        /// are known yet
        /// </param>
        /// <returns>query result for live track</returns>
        [Get("/api/LiveTrack?id={id}&time={time}")]
        Task<LiveTrackQueryResult> GetLiveTrackDataAsync(
            [AliasAs("id")] string liveTrackId,
            [AliasAs("time")] DateTimeOffset? lastTrackPointTime);
    }
}
