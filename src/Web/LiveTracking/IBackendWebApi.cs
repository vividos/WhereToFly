using Refit;
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
        /// <returns>query result for live track</returns>
        [Get("/api/LiveTrack?id={id}")]
        Task<LiveTrackQueryResult> GetLiveTrackDataAsync([AliasAs("id")] string liveTrackId);
    }
}
