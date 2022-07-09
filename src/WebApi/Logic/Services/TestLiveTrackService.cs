using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;

[assembly: InternalsVisibleTo("WhereToFly.WebApi.Logic.Test")]

namespace WhereToFly.WebApi.Logic.Services
{
    /// <summary>
    /// Data service for test live tracking data.
    /// The test live tracking generates a track where a simulated glider glides to a point for
    /// a minute, then thermals up for a minute, then glides to the starting point for a minute.
    /// </summary>
    public static class TestLiveTrackService
    {
        /// <summary>
        /// Max. height in the thermalling stage
        /// </summary>
        private const double ThermallingMaxHeight = 2050.0;

        /// <summary>
        /// Number of turns per minute while thermalling
        /// </summary>
        private const double TurnsPerMinute = 4;

        /// <summary>
        /// Radius of thermalling circle
        /// </summary>
        private const double ThermallingCircleRadius = 50.0;

        /// <summary>
        /// Number of minutes to generate in advance to now
        /// </summary>
        private const int NumMinutesToGenerate = 5;

        /// <summary>
        /// Starting point of flying to thermal
        /// </summary>
        private static readonly MapPoint StartingPoint = new(47.6735110, 11.9060494, 1850);

        /// <summary>
        /// Thermalling circle center point
        /// </summary>
        private static readonly MapPoint ThermallingPoint = new(47.6826, 11.8975, 1500);

        /// <summary>
        /// Test track stage
        /// </summary>
        private enum Stage
        {
            /// <summary>
            /// Gliding to thermal
            /// </summary>
            GlideToThermal = 0,

            /// <summary>
            /// Thermalling stage
            /// </summary>
            Thermalling = 1,

            /// <summary>
            /// Gloding to turnpoint after thermalling
            /// </summary>
            GlideToTurnpoint = 2,
        }

        /// <summary>
        /// Returns a live waypoint query result for the test live tracking
        /// </summary>
        /// <param name="id">live track ID</param>
        /// <returns>live waypoint query result</returns>
        public static LiveWaypointQueryResult GetPositionQueryResult(string id)
        {
            var now = DateTimeOffset.Now;

            return new LiveWaypointQueryResult
            {
                Data = GetTestWaypointData(id, now),
                NextRequestDate = now.AddMinutes(3.0),
            };
        }

        /// <summary>
        /// Gets test waypoint data by generating the track for the current minute and returning
        /// the track point for the current second.
        /// </summary>
        /// <param name="id">live track ID</param>
        /// <param name="now">current time</param>
        /// <returns>live waypoint data</returns>
        private static LiveWaypointData GetTestWaypointData(string id, DateTimeOffset now)
        {
            Stage currentStage = (Stage)(now.Minute % 3);

            var startTime = now.AddSeconds(-now.Second);

            Track track = GenerateTrackPoints(currentStage, startTime);
            Debug.Assert(track.TrackPoints.Count == 60, "must have exactly generated 60 track points");

            TrackPoint trackPoint = track.TrackPoints[now.Second % 60];

            return new LiveWaypointData
            {
                ID = id,
                TimeStamp = DateTimeOffset.Now,
                Longitude = trackPoint.Longitude,
                Latitude = trackPoint.Latitude,
                Altitude = trackPoint.Altitude.Value,
                Name = "Live tracking test position",
                Description = $"Current live tracking stage: {currentStage}",
                DetailsLink = string.Empty,
            };
        }

        /// <summary>
        /// Returns a new live tracking query result
        /// </summary>
        /// <param name="id">live track ID</param>
        /// <returns>live track query result</returns>
        public static LiveTrackQueryResult GetLiveTrackingQueryResult(string id)
        {
            var now = DateTimeOffset.Now;
            return new LiveTrackQueryResult
            {
                Data = GetLiveTrackingData(id, now),
                NextRequestDate = now.AddMinutes(3.0),
            };
        }

        /// <summary>
        /// Generates new live tracking data for the current time
        /// </summary>
        /// <param name="id">live track ID</param>
        /// <param name="now">current time</param>
        /// <returns>live track data</returns>
        internal static LiveTrackData GetLiveTrackingData(string id, DateTimeOffset now)
        {
            var track = GenerateLiveTrackData(now);

            DateTimeOffset trackStart = track.TrackPoints.First().Time.Value;

            var trackPoints = track.TrackPoints.Select(
                trackPoint => new LiveTrackData.LiveTrackPoint
                {
                    Latitude = trackPoint.Latitude,
                    Longitude = trackPoint.Longitude,
                    Altitude = trackPoint.Altitude ?? 0.0,
                    Offset = (trackPoint.Time.Value - trackStart).TotalSeconds,
                });

            return new LiveTrackData
            {
                ID = id,
                Name = "Live tracking test track",
                Description = "This is a live tracking test track for testing purposes.",
                TrackStart = trackStart,
                TrackPoints = trackPoints.ToArray(),
            };
        }

        /// <summary>
        /// Creates new live tracking track based on the current time
        /// </summary>
        /// <param name="now">current time</param>
        /// <returns>track with some track points</returns>
        internal static Track GenerateLiveTrackData(DateTimeOffset now)
        {
            var track = new Track();
            for (int minutes = 0; minutes < NumMinutesToGenerate; minutes++)
            {
                Stage currentStage = (Stage)((now.Minute + minutes) % 3);

                var startTime = now.AddSeconds(-now.Second).AddMinutes(minutes);

                Track minuteTrack = GenerateTrackPoints(currentStage, startTime);
                Debug.Assert(minuteTrack.TrackPoints.Count == 60, "must have exactly generated 60 track points");

                track.JoinTrackPoints(minuteTrack);
            }

            track.RemoveTrackPointsBefore(now.AddMinutes(-1.0));
            track.RemoveTrackPointsAfter(now.AddMinutes(NumMinutesToGenerate));

            return track;
        }

        /// <summary>
        /// Generates track points for a specific stage, beginning at a start time
        /// </summary>
        /// <param name="stage">live tracking stage</param>
        /// <param name="startTime">start time of stage</param>
        /// <returns>generated track</returns>
        private static Track GenerateTrackPoints(Stage stage, DateTimeOffset startTime)
        {
            var track = new Track();

            var thermallingTopPoint = ThermallingPoint.Offset(
                0.0,
                0.0,
                ThermallingMaxHeight - ThermallingPoint.Altitude.Value);

            double thermallingStartAngleInDegrees =
                (StartingPoint.CourseTo(ThermallingPoint) + 180.0) % 360.0;

            switch (stage)
            {
                case Stage.GlideToThermal:
                    InterpolateTrack(track, StartingPoint, ThermallingPoint, startTime);
                    break;

                case Stage.Thermalling:
                    CalculateThermallingTrack(
                        track,
                        ThermallingPoint,
                        thermallingTopPoint,
                        thermallingStartAngleInDegrees,
                        startTime);
                    break;

                case Stage.GlideToTurnpoint:
                    InterpolateTrack(track, thermallingTopPoint, StartingPoint, startTime);
                    break;

                default:
                    Debug.Assert(false, "invalid stage");
                    break;
            }

            return track;
        }

        /// <summary>
        /// Interpolates track points between two map points and ass them to the track.
        /// </summary>
        /// <param name="track">track to add points to</param>
        /// <param name="startingPoint">starting point of glide</param>
        /// <param name="endingPoint">ending point</param>
        /// <param name="startTime">start time</param>
        private static void InterpolateTrack(
            Track track,
            MapPoint startingPoint,
            MapPoint endingPoint,
            DateTimeOffset startTime)
        {
            const int NumSeconds = 60;

            for (int seconds = 0; seconds < NumSeconds; seconds++)
            {
                double delta = (double)seconds / NumSeconds;

                track.TrackPoints.Add(
                    new TrackPoint(
                        WhereToFly.Shared.Base.Math.Interpolate(startingPoint.Latitude, endingPoint.Latitude, delta),
                        WhereToFly.Shared.Base.Math.Interpolate(startingPoint.Longitude, endingPoint.Longitude, delta),
                        WhereToFly.Shared.Base.Math.Interpolate(startingPoint.Altitude.Value, endingPoint.Altitude.Value, delta),
                        null)
                    {
                        Time = startTime.AddSeconds(seconds),
                    });
            }
        }

        /// <summary>
        /// Calculates track points to simulate thermalling up to a top point
        /// </summary>
        /// <param name="track">track to add points to</param>
        /// <param name="thermallingPoint">thermilling base point</param>
        /// <param name="thermallingTopPoint">thermalling top point</param>
        /// <param name="thermallingStartAngleInDegrees">start angle of thermalling</param>
        /// <param name="startTime">start time</param>
        private static void CalculateThermallingTrack(
            Track track,
            MapPoint thermallingPoint,
            MapPoint thermallingTopPoint,
            double thermallingStartAngleInDegrees,
            DateTimeOffset startTime)
        {
            double secondsForOneTurn = 60.0 / TurnsPerMinute;

            // offset base point into the previous flight direction
            MapPoint thermallingBasePoint = thermallingPoint.PolarOffset(
                ThermallingCircleRadius,
                thermallingStartAngleInDegrees + 180.0,
                0.0);

            for (int seconds = 0; seconds < 60; seconds++)
            {
                double delta = seconds / 60.0;

                double secondsInThisCircle = seconds % secondsForOneTurn;

                double currentThermallingCircleAngle =
                    thermallingStartAngleInDegrees +
                    (360.0 * (secondsInThisCircle / secondsForOneTurn));

                currentThermallingCircleAngle = currentThermallingCircleAngle % 360.0;

                var currentThermallingPoint = thermallingBasePoint.PolarOffset(
                    ThermallingCircleRadius,
                    currentThermallingCircleAngle,
                    0.0);

                track.TrackPoints.Add(
                    new TrackPoint(
                        currentThermallingPoint.Latitude,
                        currentThermallingPoint.Longitude,
                        WhereToFly.Shared.Base.Math.Interpolate(thermallingPoint.Altitude.Value, thermallingTopPoint.Altitude.Value, delta),
                        null)
                    {
                        Time = startTime.AddSeconds(seconds),
                    });
            }
        }
    }
}
