using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Service for refreshing live waypoint data and live track data
    /// </summary>
    public class LiveDataRefreshService
    {
        /// <summary>
        /// Update info for queueing live data updates
        /// </summary>
        private sealed class LiveDataUpdateInfo
        {
            /// <summary>
            /// Update time for this given live waypoint/track
            /// </summary>
            public DateTimeOffset UpdateTime { get; set; }

            /// <summary>
            /// Location or track ID of live waypoint/track to update
            /// </summary>
            public string LocationOrTrackId { get; set; }
        }

        /// <summary>
        /// Lock for liveWaypointMap, liveTrackMap, liveTrackLastTrackPointTimeMap,
        /// nextUpdateQueue and nextPossibleUpdateMap
        /// </summary>
        private readonly object dataLock = new();

        /// <summary>
        /// Mapping of live waypoint IDs and their location objects
        /// </summary>
        private readonly Dictionary<string, Location> liveWaypointMap = new();

        /// <summary>
        /// Mapping of live track IDs and their track objects
        /// </summary>
        private readonly Dictionary<string, Track> liveTrackMap = new();

        /// <summary>
        /// Mapping of live track IDs and the last track point time
        /// </summary>
        private readonly Dictionary<string, DateTimeOffset> liveTrackLastTrackPointTimeMap =
            new();

        /// <summary>
        /// Queue with all updates to be due
        /// </summary>
        private readonly Queue<LiveDataUpdateInfo> nextUpdateQueue = new();

        /// <summary>
        /// Map with times when a next update can be scheduled
        /// </summary>
        private readonly Dictionary<string, DateTimeOffset> nextPossibleUpdateMap = new();

        /// <summary>
        /// Timer to schedule updates
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Data service instance
        /// </summary>
        public IDataService DataService { get; set; }

        /// <summary>
        /// Delegate of event that is triggered when live waypoint or track data was updated
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        public delegate void OnUpdateLiveData(object sender, LiveDataUpdateEventArgs args);

        /// <summary>
        /// Event that is triggered when data for a live waypoint or a live track was updated
        /// </summary>
        public event OnUpdateLiveData UpdateLiveData;

        /// <summary>
        /// Creates a new live data refresh service
        /// </summary>
        public LiveDataRefreshService()
        {
        }

        /// <summary>
        /// Adds location list, to be checked for live waypoints to be updated periodically.
        /// </summary>
        /// <param name="locationList">location list</param>
        public void AddLiveWaypointList(IEnumerable<Location> locationList)
        {
            var liveWaypointLocationList = locationList.Where(location => location.Type == LocationType.LiveWaypoint);

            if (!liveWaypointLocationList.Any())
            {
                return;
            }

            Debug.WriteLine($"LiveDataRefreshService: adding list of {liveWaypointLocationList.Count()} live waypoints");

            lock (this.dataLock)
            {
                foreach (Location liveWaypointLocation in liveWaypointLocationList)
                {
                    if (!this.liveWaypointMap.ContainsKey(liveWaypointLocation.Id))
                    {
                        this.liveWaypointMap.Add(liveWaypointLocation.Id, liveWaypointLocation);
                    }
                }
            }

            this.ScheduleUpdates();
        }

        /// <summary>
        /// Adds live waypoint to be updated periodically.
        /// </summary>
        /// <param name="liveWaypointLocation">live waypoint location to add</param>
        public void AddLiveWaypoint(Location liveWaypointLocation)
        {
            lock (this.dataLock)
            {
                if (this.liveWaypointMap.ContainsKey(liveWaypointLocation.Id))
                {
                    this.liveWaypointMap[liveWaypointLocation.Id] = liveWaypointLocation;
                }
                else
                {
                    this.liveWaypointMap.Add(liveWaypointLocation.Id, liveWaypointLocation);
                }
            }

            this.ScheduleUpdates();
        }

        /// <summary>
        /// Removes live waypoint from being updated.
        /// </summary>
        /// <param name="liveWaypointLocationId">live waypoint ID to remove</param>
        public void RemoveLiveWaypoint(string liveWaypointLocationId)
        {
            lock (this.dataLock)
            {
                this.liveWaypointMap.Remove(liveWaypointLocationId);
                this.nextPossibleUpdateMap.Remove(liveWaypointLocationId);
            }

            this.ScheduleUpdates();
        }

        /// <summary>
        /// Adds live track, to be updated periodically.
        /// </summary>
        /// <param name="liveTrack">live track to add</param>
        public void AddLiveTrack(Track liveTrack)
        {
            lock (this.dataLock)
            {
                if (this.liveTrackMap.ContainsKey(liveTrack.Id))
                {
                    this.liveTrackMap[liveTrack.Id] = liveTrack;
                }
                else
                {
                    this.liveTrackMap.Add(liveTrack.Id, liveTrack);
                }

                DateTimeOffset? lastTrackPointTime =
                    liveTrack.TrackPoints.LastOrDefault(
                        trackPoint => trackPoint.Time != null)?.Time;

                if (lastTrackPointTime != null)
                {
                    this.liveTrackLastTrackPointTimeMap[liveTrack.Id] =
                        lastTrackPointTime.Value;
                }
            }

            this.ScheduleUpdates();
        }

        /// <summary>
        /// Removes live track from being updated.
        /// </summary>
        /// <param name="liveTrackId">live track ID to remove</param>
        public void RemoveLiveTrack(string liveTrackId)
        {
            lock (this.dataLock)
            {
                this.liveTrackMap.Remove(liveTrackId);
                this.nextPossibleUpdateMap.Remove(liveTrackId);
            }

            this.ScheduleUpdates();
        }

        /// <summary>
        /// Checks all live waypoints/tracks for a next possible update and schedules updating the
        /// live waypoints/tracks.
        /// </summary>
        private void ScheduleUpdates()
        {
            Debug.WriteLine("LiveDataRefreshService: scheduling updates");

            lock (this.dataLock)
            {
                DateTimeOffset nextUpdateTime = DateTimeOffset.MaxValue;
                string nextLocationOrTrackId = null;
                foreach (var item in this.nextPossibleUpdateMap)
                {
                    if (item.Value < nextUpdateTime)
                    {
                        nextUpdateTime = item.Value;
                        nextLocationOrTrackId = item.Key;
                    }
                }

                if (nextLocationOrTrackId != null)
                {
                    this.EnqueueNextUpdate(nextLocationOrTrackId, nextUpdateTime);
                    this.StartTimer(nextUpdateTime);
                }
                else if (this.liveWaypointMap.Any() || this.liveTrackMap.Any())
                {
                    // no locations or tracks in the map; update all
                    Debug.WriteLine("LiveDataRefreshService: schedule update for all live waypoints and live tracks");

                    foreach (string liveWaypointId in this.liveWaypointMap.Keys)
                    {
                        this.EnqueueNextUpdate(liveWaypointId, DateTimeOffset.Now);
                    }

                    foreach (string liveTrackId in this.liveTrackMap.Keys)
                    {
                        this.EnqueueNextUpdate(liveTrackId, DateTimeOffset.Now);
                    }

                    Task.Run(this.CheckLiveWaypointsAndTracksAsync);
                }
                else
                {
                    nextUpdateTime = DateTimeOffset.Now + TimeSpan.FromMinutes(1.0);
                    this.StartTimer(nextUpdateTime);
                }
            }
        }

        /// <summary>
        /// Enqueues next update for given location or track ID
        /// </summary>
        /// <param name="locationOrTrackId">location or track ID to enqueue update</param>
        /// <param name="nextUpdateTime">time of next update</param>
        private void EnqueueNextUpdate(string locationOrTrackId, DateTimeOffset nextUpdateTime)
        {
            lock (this.dataLock)
            {
                if (this.nextUpdateQueue.Any(updateInfo => updateInfo.LocationOrTrackId == locationOrTrackId))
                {
                    return; // already scheduled
                }

                this.nextUpdateQueue.Enqueue(new LiveDataUpdateInfo
                {
                    LocationOrTrackId = locationOrTrackId,
                    UpdateTime = nextUpdateTime,
                });
            }
        }

        /// <summary>
        /// Starts time to check live waypoints and live tracks at next given update time
        /// </summary>
        /// <param name="nextUpdateTime">next time an update should be carried out</param>
        private void StartTimer(DateTimeOffset nextUpdateTime)
        {
            Debug.WriteLine($"LiveDataRefreshService: starting timer for next update on {nextUpdateTime}");

            TimeSpan dueTimeSpan = nextUpdateTime - DateTimeOffset.Now;

            if (dueTimeSpan < TimeSpan.Zero)
            {
                Task.Run(this.CheckLiveWaypointsAndTracksAsync);
                return;
            }

            this.timer = new Timer(
                async (state) =>
                {
                    await this.CheckLiveWaypointsAndTracksAsync();
                },
                null,
                dueTimeSpan,
                TimeSpan.Zero);
        }

        /// <summary>
        /// Stops timer to update live waypoints
        /// </summary>
        public void StopTimer()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        /// <summary>
        /// Resumes timer (by just scheduling the next timer update)
        /// </summary>
        public void ResumeTimer()
        {
            this.ScheduleUpdates();
        }

        /// <summary>
        /// Clears all live waypoints in the service, e.g. when user cleared location list
        /// </summary>
        public void ClearLiveWaypointList()
        {
            lock (this.dataLock)
            {
                // don't clear nextPossibleUpdateMap, as the info might be needed again when
                // re -adding some live waypoints
                this.liveWaypointMap.Clear();
                this.nextUpdateQueue.Clear();
            }
        }

        /// <summary>
        /// Clears all live tracks in the service, e.g. when user cleared track list
        /// </summary>
        public void ClearLiveTrackList()
        {
            lock (this.dataLock)
            {
                // don't clear nextPossibleUpdateMap, as the info might be needed again when
                // re -adding some live waypoints
                this.liveTrackMap.Clear();
                this.nextUpdateQueue.Clear();
            }
        }

        /// <summary>
        /// Checks all live waypoints in queue that should be updated.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task CheckLiveWaypointsAndTracksAsync()
        {
            this.StopTimer();

            Debug.WriteLine("LiveDataRefreshService: checking all live waypoints in queue");

            LiveDataUpdateInfo updateInfo;
            while ((updateInfo = this.GetNextUpdateInfo()) != null)
            {
                bool isLocation = false;
                DateTimeOffset? lastTrackPointTime = null;
                lock (this.dataLock)
                {
                    isLocation = this.liveWaypointMap.ContainsKey(updateInfo.LocationOrTrackId);

                    if (!isLocation)
                    {
                        if (this.liveTrackLastTrackPointTimeMap.ContainsKey(updateInfo.LocationOrTrackId))
                        {
                            lastTrackPointTime = this.liveTrackLastTrackPointTimeMap[updateInfo.LocationOrTrackId];
                        }
                        else if (this.liveTrackMap.TryGetValue(updateInfo.LocationOrTrackId, out Track liveTrack))
                        {
                            lastTrackPointTime =
                                liveTrack.TrackPoints.LastOrDefault(
                                    trackPoint => trackPoint.Time != null)?.Time;
                        }
                    }
                }

                if (isLocation)
                {
                    await this.UpdateLiveWaypointAsync(updateInfo.LocationOrTrackId);
                }
                else
                {
                    await this.UpdateLiveTrackAsync(
                        updateInfo.LocationOrTrackId,
                        lastTrackPointTime);
                }
            }

            this.ScheduleUpdates();
        }

        /// <summary>
        /// Retrieves next update info object from queue, or null when there is no update due or
        /// the queue is empty.
        /// </summary>
        /// <returns>update info object, or null when no object is available</returns>
        private LiveDataUpdateInfo GetNextUpdateInfo()
        {
            lock (this.dataLock)
            {
                if (this.nextUpdateQueue.Count > 0 &&
                    this.nextUpdateQueue.Peek().UpdateTime <= DateTimeOffset.Now)
                {
                    return this.nextUpdateQueue.Dequeue();
                }

                return null;
            }
        }

        /// <summary>
        /// Updates live waypoint by getting live waypoint data and notify all event subjects.
        /// </summary>
        /// <param name="liveWaypointId">live waypoint ID to update</param>
        /// <returns>task to wait on</returns>
        private async Task UpdateLiveWaypointAsync(string liveWaypointId)
        {
            Debug.WriteLine($"LiveDataRefreshService: updating live waypoint for {liveWaypointId}");

            LiveWaypointQueryResult queryResult = null;
            try
            {
                queryResult = await this.DataService.GetLiveWaypointDataAsync(liveWaypointId);
            }
            catch (Exception ex)
            {
                // ignore exception
                Debug.WriteLine($"LiveDataRefreshService: exception occured: {ex}");
            }

            if (queryResult == null)
            {
                // try again in a minute
                lock (this.dataLock)
                {
                    this.nextPossibleUpdateMap[liveWaypointId] = DateTimeOffset.Now + TimeSpan.FromMinutes(1.0);
                }

                return;
            }

            if (queryResult.Data != null)
            {
                this.UpdateLiveData?.Invoke(
                    this,
                    new LiveDataUpdateEventArgs
                    {
                        WaypointData = queryResult.Data,
                    });
            }

            var nextUpdate = queryResult.NextRequestDate;
            lock (this.dataLock)
            {
                this.nextPossibleUpdateMap[liveWaypointId] = nextUpdate;
            }
        }

        /// <summary>
        /// Updates live track by getting live track data and notify all event subjects.
        /// </summary>
        /// <param name="liveTrackId">live track ID to update</param>
        /// <param name="lastTrackPointTime">
        /// last track point that the client already has received, or null when no track points
        /// are known yet
        /// </param>
        /// <returns>task to wait on</returns>
        private async Task UpdateLiveTrackAsync(
            string liveTrackId,
            DateTimeOffset? lastTrackPointTime)
        {
            Debug.WriteLine($"LiveDataRefreshService: updating live track for {liveTrackId}");

            LiveTrackQueryResult queryResult = null;
            try
            {
                queryResult = await this.DataService.GetLiveTrackDataAsync(
                    liveTrackId,
                    lastTrackPointTime);
            }
            catch (Exception ex)
            {
                // ignore exception
                Debug.WriteLine($"LiveDataRefreshService: exception occured: {ex}");
            }

            if (queryResult == null)
            {
                // try again in a minute
                lock (this.dataLock)
                {
                    this.nextPossibleUpdateMap[liveTrackId] = DateTimeOffset.Now + TimeSpan.FromMinutes(1.0);
                }

                return;
            }

            DateTimeOffset? nextLastTrackPointTime = null;

            if (queryResult.Data != null)
            {
                this.UpdateLiveData?.Invoke(
                    this,
                    new LiveDataUpdateEventArgs
                    {
                        TrackData = queryResult.Data,
                    });

                if (queryResult.Data.TrackPoints.Any())
                {
                    double offset = queryResult.Data.TrackPoints.Last().Offset;
                    nextLastTrackPointTime = queryResult.Data.TrackStart.AddSeconds(offset);
                }
            }

            var nextUpdate = queryResult.NextRequestDate;
            lock (this.dataLock)
            {
                this.nextPossibleUpdateMap[liveTrackId] = nextUpdate;

                if (nextLastTrackPointTime != null)
                {
                    this.liveTrackLastTrackPointTimeMap[liveTrackId] =
                        nextLastTrackPointTime.Value;
                }
            }
        }
    }
}
