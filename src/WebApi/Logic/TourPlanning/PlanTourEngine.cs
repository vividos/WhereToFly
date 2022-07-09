using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using WhereToFly.Shared.Model;

[assembly: InternalsVisibleTo("WhereToFly.WebApi.UnitTest")]

namespace WhereToFly.WebApi.Logic.TourPlanning
{
    /// <summary>
    /// Engine to plan hiking tours using a graph with tracks and waypoints. First, a graph must
    /// be loaded using LoadGraph(), using a specially prepared KML file. Then a tour can be
    /// planned using PlanTour().
    /// </summary>
    public class PlanTourEngine
    {
        /// <summary>
        /// Tour graph with all waypoints and tracks that can be visited
        /// </summary>
        private readonly BidirectionalGraph<WaypointInfo, TrackInfo> tourGraph =
            new(allowParallelEdges: true);

        /// <summary>
        /// Loads graph by using KML file from given stream
        /// </summary>
        /// <param name="kmlStream">kml file stream</param>
        public void LoadGraph(Stream kmlStream)
        {
            var loader = new TourGraphLoader(this);
            loader.Load(kmlStream);
        }

        /// <summary>
        /// Adds new waypoint to the graph. Used by the graph loader.
        /// </summary>
        /// <param name="waypointInfo">waypoint info to add</param>
        internal void AddWaypoint(WaypointInfo waypointInfo) => this.tourGraph.AddVertex(waypointInfo);

        /// <summary>
        /// Adds new track to the graph. Used by the graph loader.
        /// </summary>
        /// <param name="trackInfo">track info to add</param>
        internal void AddTrack(TrackInfo trackInfo) => this.tourGraph.AddEdge(trackInfo);

        /// <summary>
        /// Finds waypoint info for given waypoint ID
        /// </summary>
        /// <param name="waypointId">waypoint ID to find</param>
        /// <returns>waypoint info, or null when not found</returns>
        internal WaypointInfo FindWaypointInfo(string waypointId)
        {
            return this.tourGraph.Vertices.FirstOrDefault(x => x.Id == waypointId);
        }

        /// <summary>
        /// Finds waypoint info for given waypoint ID, and creates it if it doesn't exist. This
        /// can be used to create "intermediate" waypoints that act as node between two edges.
        /// </summary>
        /// <param name="waypointId">waypoint ID to find</param>
        /// <returns>waypoint info</returns>
        internal WaypointInfo FindOrCreateWaypointInfo(string waypointId)
        {
            var waypointInfo = this.FindWaypointInfo(waypointId);

            if (waypointInfo == null)
            {
                // create an intermediate waypoint that isn't used as tour point
                waypointInfo = new WaypointInfo
                {
                    Id = waypointId,
                    Description = string.Empty,
                };

                this.tourGraph.AddVertex(waypointInfo);
            }

            return waypointInfo;
        }

        /// <summary>
        /// Plans a tour with given waypoints
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <returns>planned tour</returns>
        public PlannedTour PlanTour(PlanTourParameters planTourParameters)
        {
            this.CheckTourPlanningArguments(planTourParameters);

            var vertexList = (from waypointId in planTourParameters.WaypointIdList
                              select this.FindWaypointInfo(waypointId)).ToList();

            var tour = new PlannedTour();
            var descriptionText = new StringBuilder();
            var totalDuration = TimeSpan.Zero;

            for (int index = 1; index < vertexList.Count; index++)
            {
                var source = vertexList[index - 1];
                var target = vertexList[index];

                this.PlanSingleTourStep(source, target, tour, descriptionText, ref totalDuration);

                if (index + 1 == vertexList.Count)
                {
                    descriptionText.Append($"Ende-Wegpunkt: {target.Description}\n");
                }
            }

            tour.Description = descriptionText.ToString();
            tour.TotalDuration = totalDuration;

            return tour;
        }

        /// <summary>
        /// Plans single tour step between two waypoints
        /// </summary>
        /// <param name="source">source waypoint</param>
        /// <param name="target">target waypoint</param>
        /// <param name="tour">planned tour object</param>
        /// <param name="descriptionText">description text string builder</param>
        /// <param name="totalDuration">reference to total duration</param>
        private void PlanSingleTourStep(WaypointInfo source, WaypointInfo target, PlannedTour tour, StringBuilder descriptionText, ref TimeSpan totalDuration)
        {
            Debug.WriteLine($"finding shortest path between {source} and {target}");

            Func<TrackInfo, double> edgeCost = GetEdgeWeightShortestTime;

            TryFunc<WaypointInfo, IEnumerable<TrackInfo>> tryGetPaths =
                this.tourGraph.ShortestPathsDijkstra(edgeCost, source);

            if (tryGetPaths(target, out IEnumerable<TrackInfo> path))
            {
                foreach (var edge in path)
                {
                    Debug.WriteLine(edge);

                    tour.TourEntriesList.Add(
                        new PlannedTourEntry
                        {
                            StartWaypointId = edge.Source.Id,
                            EndWaypointId = edge.Target.Id,
                            TrackStartIndex = tour.MapPointList.Count,
                            Duration = edge.Duration,
                        });

                    tour.MapPointList.AddRange(edge.MapPointList);

                    if (!string.IsNullOrEmpty(edge.Source.Description))
                    {
                        descriptionText.Append($"Wegpunkt: {edge.Source.Description} \n\n");
                    }

                    descriptionText.Append($"{edge.Description}\n\n");

                    totalDuration += edge.Duration;
                }
            }

            Debug.WriteLine("finished.");
            Debug.WriteLine(string.Empty);
        }

        /// <summary>
        /// Gets an edge weight for track info to optimize for shortest time traveled.
        /// </summary>
        /// <param name="edge">edge to use</param>
        /// <returns>edge weight</returns>
        private static double GetEdgeWeightShortestTime(TrackInfo edge)
        {
            return edge.Duration.TotalMinutes;
        }

        /// <summary>
        /// Checks all tour planning arguments for validity
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        private void CheckTourPlanningArguments(PlanTourParameters planTourParameters)
        {
            if (planTourParameters == null)
            {
                throw new ArgumentNullException(nameof(planTourParameters));
            }

            if (planTourParameters.WaypointIdList.Count < 2)
            {
                throw new InvalidOperationException("there must be at least two waypoints for tour planning");
            }

            // check if one of the waypoints can't be found
            string invalidWaypointId = planTourParameters.WaypointIdList.FirstOrDefault(
                waypointId => this.FindWaypointInfo(waypointId) == null);

            if (invalidWaypointId != null)
            {
                throw new ArgumentException($"invalid waypoint id for tour planning: {invalidWaypointId}");
            }
        }
    }
}
