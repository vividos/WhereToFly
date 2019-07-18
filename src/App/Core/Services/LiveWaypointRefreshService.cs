using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Service for refreshing live waypoints and their data
    /// </summary>
    public class LiveWaypointRefreshService
    {
        /// <summary>
        /// Update info for queueing live waypoint updates
        /// </summary>
        private class LiveWaypointUpdateInfo
        {
            /// <summary>
            /// Update time for this given live waypoint
            /// </summary>
            public DateTimeOffset UpdateTime { get; set; }

            /// <summary>
            /// Location ID of live waypoint to update
            /// </summary>
            public string LocationId { get; set; }
        }

        /// <summary>
        /// Lock for liveWaypointMap, nextUpdateQueue and nextPossibleUpdateMap
        /// </summary>
        private readonly object dataLock = new object();

        /// <summary>
        /// Mapping of live waypoint IDs and their location objects
        /// </summary>
        private readonly Dictionary<string, Location> liveWaypointMap = new Dictionary<string, Location>();

        /// <summary>
        /// Queue with all updates to be due
        /// </summary>
        private readonly Queue<LiveWaypointUpdateInfo> nextUpdateQueue = new Queue<LiveWaypointUpdateInfo>();

        /// <summary>
        /// Map with times when a next update can be scheduled
        /// </summary>
        private readonly Dictionary<string, DateTimeOffset> nextPossibleUpdateMap = new Dictionary<string, DateTimeOffset>();

        /// <summary>
        /// Timer to schedule updates
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Data service instance
        /// </summary>
        public IDataService DataService { get; set; }

        /// <summary>
        /// Delegate of event that is triggered when live waypoint data was updated
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        public delegate void OnUpdateLiveWaypoint(object sender, LiveWaypointUpdateEventArgs args);

        /// <summary>
        /// Event that is triggered when data for a live waypoint was updated
        /// </summary>
        public event OnUpdateLiveWaypoint UpdateLiveWaypoint;

        /// <summary>
        /// Creates a new live waypoint refresh service
        /// </summary>
        public LiveWaypointRefreshService()
        {
        }

        /// <summary>
        /// Updates live waypoints to update by list of locations, e.g. when a new list is loaded.
        /// </summary>
        /// <param name="locationList">location list</param>
        public void UpdateLiveWaypointList(List<Location> locationList)
        {
            Debug.WriteLine("LiveWaypointRefreshService: updating list of live waypoints");

            var addedIds = new HashSet<string>();

            lock (this.dataLock)
            {
                var liveWaypointLocationList = locationList.Where(location => location.Type == LocationType.LiveWaypoint);
                foreach (Location liveWaypointLocation in liveWaypointLocationList)
                {
                    if (!this.liveWaypointMap.ContainsKey(liveWaypointLocation.Id))
                    {
                        this.AddLiveWaypoint(liveWaypointLocation);
                        addedIds.Add(liveWaypointLocation.Id);
                    }
                }

                var idsToRemove = new HashSet<string>();

                foreach (string liveWaypointId in this.liveWaypointMap.Keys)
                {
                    if (!addedIds.Contains(liveWaypointId))
                    {
                        idsToRemove.Add(liveWaypointId);
                    }
                }

                foreach (string liveWaypointId in idsToRemove)
                {
                    this.RemoveLiveWaypoint(liveWaypointId);
                }

                this.ScheduleUpdates();
            }
        }

        /// <summary>
        /// Adds live waypoint to the live waypoint map
        /// </summary>
        /// <param name="liveWaypointLocation">live waypoint location to add</param>
        private void AddLiveWaypoint(Location liveWaypointLocation)
        {
            lock (this.dataLock)
            {
                this.liveWaypointMap.Add(liveWaypointLocation.Id, liveWaypointLocation);
            }
        }

        /// <summary>
        /// Removes live waypoint from the live waypoint map
        /// </summary>
        /// <param name="liveWaypointLocationId">live waypoint ID to remove</param>
        private void RemoveLiveWaypoint(string liveWaypointLocationId)
        {
            lock (this.dataLock)
            {
                this.liveWaypointMap.Remove(liveWaypointLocationId);
                this.nextPossibleUpdateMap.Remove(liveWaypointLocationId);
            }
        }

        /// <summary>
        /// Checks all live waypoints for a next possible update and schedules updating the live
        /// waypoints.
        /// </summary>
        private void ScheduleUpdates()
        {
            Debug.WriteLine("LiveWaypointRefreshService: scheduling updates");

            lock (this.dataLock)
            {
                DateTimeOffset nextUpdateTime = DateTimeOffset.MaxValue;
                string nextLocationId = null;
                foreach (var item in this.nextPossibleUpdateMap)
                {
                    if (item.Value < nextUpdateTime)
                    {
                        nextUpdateTime = item.Value;
                        nextLocationId = item.Key;
                    }
                }

                if (nextLocationId != null)
                {
                    this.nextUpdateQueue.Enqueue(new LiveWaypointUpdateInfo
                    {
                        LocationId = nextLocationId,
                        UpdateTime = nextUpdateTime
                    });

                    this.StartTimer(nextUpdateTime);
                }
                else if (this.liveWaypointMap.Any())
                {
                    // no locations in the map; update all
                    Debug.WriteLine("LiveWaypointRefreshService: schedule update for all live waypoints");

                    foreach (var liveWaypointId in this.liveWaypointMap.Keys)
                    {
                        this.nextUpdateQueue.Enqueue(new LiveWaypointUpdateInfo
                        {
                            LocationId = liveWaypointId,
                            UpdateTime = DateTimeOffset.Now
                        });
                    }

                    Task.Run(this.CheckLiveWaypointsAsync);
                }
                else
                {
                    nextUpdateTime = DateTimeOffset.Now + TimeSpan.FromMinutes(1.0);
                    this.StartTimer(nextUpdateTime);
                }
            }
        }

        /// <summary>
        /// Starts time to check live waypoints at next given update time
        /// </summary>
        /// <param name="nextUpdateTime">next time an update should be carried out</param>
        private void StartTimer(DateTimeOffset nextUpdateTime)
        {
            Debug.WriteLine($"LiveWaypointRefreshService: starting timer for next update on {nextUpdateTime}");

            TimeSpan dueTimeSpan = nextUpdateTime - DateTimeOffset.Now;

            if (dueTimeSpan < TimeSpan.Zero)
            {
                Task.Run(this.CheckLiveWaypointsAsync);
                return;
            }

            this.timer = new Timer(
                async (state) =>
                {
                    await this.CheckLiveWaypointsAsync();
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
        /// Checks all live waypoints in queue that should be updated.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task CheckLiveWaypointsAsync()
        {
            this.StopTimer();

            Debug.WriteLine("LiveWaypointRefreshService: checking all live waypoints in queue");

            LiveWaypointUpdateInfo updateInfo;
            while ((updateInfo = this.GetNextUpdateInfo()) != null)
            {
                await this.UpdateLiveWaypointAsync(updateInfo.LocationId);
            }

            this.ScheduleUpdates();
        }

        /// <summary>
        /// Retrieves next update info object from queue, or null when there is no update due or
        /// the queue is empty.
        /// </summary>
        /// <returns>update info object, or null when no object is available</returns>
        private LiveWaypointUpdateInfo GetNextUpdateInfo()
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
            Debug.WriteLine($"LiveWaypointRefreshService: updating live waypoint for {liveWaypointId}");

            LiveWaypointQueryResult data = null;
            try
            {
                data = await this.DataService.GetLiveWaypointDataAsync(liveWaypointId);
            }
            catch (Exception ex)
            {
                // ignore exception
                Debug.WriteLine($"LiveWaypointRefreshService: exception occured: {ex.ToString()}");
            }

            if (data == null)
            {
                // try again in a minute
                lock (this.dataLock)
                {
                    this.nextPossibleUpdateMap[liveWaypointId] = DateTimeOffset.Now + TimeSpan.FromMinutes(1.0);
                }

                return;
            }

            if (data.Data != null)
            {
                this.UpdateLiveWaypoint?.Invoke(
                    this,
                    new LiveWaypointUpdateEventArgs
                    {
                        Data = data.Data,
                    });
            }

            var nextUpdate = data.NextRequestDate;
            lock (this.dataLock)
            {
                this.nextPossibleUpdateMap[liveWaypointId] = nextUpdate;
            }
        }
    }
}
