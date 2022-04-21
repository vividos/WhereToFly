using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Extension methods for Track objects
    /// </summary>
    public static class TrackExtensionMethods
    {
        /// <summary>
        /// Maximum height limit used for starting values of statistics calculation
        /// </summary>
        private const int MaxHeightLimit = 10000;

        /// <summary>
        /// Calculates center point of all track points
        /// </summary>
        /// <param name="track">track touse</param>
        /// <returns>map center point</returns>
        public static MapPoint CalculateCenterPoint(this Track track)
        {
            double latitude = 0.0;
            double longitude = 0.0;
            double altitude = 0.0;

            int numAltitudePoints = 0;
            foreach (var trackPoint in track.TrackPoints)
            {
                latitude += trackPoint.Latitude;
                longitude += trackPoint.Longitude;
                if (trackPoint.Altitude.HasValue)
                {
                    altitude += trackPoint.Altitude.Value;
                    numAltitudePoints++;
                }
            }

            int numTrackPoint = track.TrackPoints.Count;

            return new MapPoint(
                numTrackPoint > 0 ? latitude / numTrackPoint : 0.0,
                numTrackPoint > 0 ? longitude / numTrackPoint : 0.0,
                numAltitudePoints > 0 ? new double?(altitude / numAltitudePoints) : null);
        }

        /// <summary>
        /// Generates Time values for every track point in time, with given start date and track
        /// point interval
        /// </summary>
        /// <param name="track">track to modify</param>
        /// <param name="startDate">start date/time</param>
        /// <param name="trackPointInterval">track point interval</param>
        public static void GenerateTrackPointTimeValues(this Track track, DateTimeOffset startDate, TimeSpan trackPointInterval)
        {
            for (int trackPointIndex = 0; trackPointIndex < track.TrackPoints.Count; trackPointIndex++)
            {
                TrackPoint trackPoint = track.TrackPoints[trackPointIndex];

                trackPoint.Time = startDate + TimeSpan.FromSeconds(trackPointIndex * trackPointInterval.TotalSeconds);
            }
        }

        /// <summary>
        /// Applies an offset to altitude values of all track points
        /// </summary>
        /// <param name="track">track to modify</param>
        /// <param name="offsetInMeters">altitude offset in meters</param>
        public static void ApplyAltitudeOffset(this Track track, double offsetInMeters)
        {
            for (int trackPointIndex = 0; trackPointIndex < track.TrackPoints.Count; trackPointIndex++)
            {
                TrackPoint trackPoint = track.TrackPoints[trackPointIndex];

                if (trackPoint.Altitude.HasValue)
                {
                    trackPoint.Altitude = trackPoint.Altitude.Value + offsetInMeters;
                }
            }
        }

        /// <summary>
        /// Adjusts a track's altitude values by using the given ground height profile. When a
        /// track point's altitude goes below the ground profile, adjust the point.
        /// The number of track points must be equal to the number of ground height profile
        /// altitudes.
        /// </summary>
        /// <param name="track">track to modify</param>
        /// <param name="groundHeightProfile">ground height profile altitudes</param>
        public static void AdjustTrackPointsByGroundProfile(this Track track, double[] groundHeightProfile)
        {
            Debug.Assert(
                track.TrackPoints.Count == groundHeightProfile.Length,
                "number of track points must be the same as the number of ground height profile altitudes");

            int modifiedPoints = 0;
            for (int pointIndex = 0; pointIndex < track.TrackPoints.Count; pointIndex++)
            {
                double delta = (track.TrackPoints[pointIndex].Altitude ?? 0.0) - groundHeightProfile[pointIndex];
                if (delta < 0.0)
                {
                    modifiedPoints++;

                    track.TrackPoints[pointIndex].Altitude =
                        groundHeightProfile[pointIndex];
                }
            }

            Debug.WriteLine($"AdjustTrackPointsByGroundProfile: {modifiedPoints} of {track.TrackPoints.Count} points changed");
        }

        /// <summary>
        /// Joins track points from another track to this track
        /// </summary>
        /// <param name="track">this track to add track points to</param>
        /// <param name="trackPointsToJoin">track to join</param>
        public static void JoinTrackPoints(this Track track, Track trackPointsToJoin)
        {
            if (!trackPointsToJoin.TrackPoints.Any())
            {
                return;
            }

            if (!track.TrackPoints.Any())
            {
                track.TrackPoints.AddRange(trackPointsToJoin.TrackPoints);
                return;
            }

            if (track.TrackPoints.First().Time > trackPointsToJoin.TrackPoints.Last().Time)
            {
                track.TrackPoints.InsertRange(0, trackPointsToJoin.TrackPoints);
                return;
            }

            if (track.TrackPoints.Last().Time < trackPointsToJoin.TrackPoints.First().Time)
            {
                track.TrackPoints.AddRange(trackPointsToJoin.TrackPoints);
                return;
            }

            throw new FormatException("tracks to join overlap!");
        }

        /// <summary>
        /// Removes all track points before a given date and time
        /// </summary>
        /// <param name="track">track to modify</param>
        /// <param name="dateBefore">before date and time</param>
        public static void RemoveTrackPointsBefore(this Track track, DateTimeOffset dateBefore)
        {
            track.TrackPoints.RemoveAll(trackPoint => trackPoint.Time < dateBefore);
        }

        /// <summary>
        /// Removes all track points after a given date and time
        /// </summary>
        /// <param name="track">track to modify</param>
        /// <param name="dateAfter">after date and time</param>
        public static void RemoveTrackPointsAfter(this Track track, DateTimeOffset dateAfter)
        {
            track.TrackPoints.RemoveAll(trackPoint => trackPoint.Time > dateAfter);
        }

        /// <summary>
        /// Calculates statistics for track
        /// </summary>
        /// <param name="track">track to use</param>
        public static void CalculateStatistics(this Track track)
        {
            track.ResetStatistics();

            CalcTrackDuration(track);

            TrackPoint previousPoint = null;
            int averageSpeedTrackPointCount = 0;

            foreach (var trackPoint in track.TrackPoints)
            {
                if (trackPoint.Altitude.HasValue)
                {
                    CalcAltitudeStatistics(track, trackPoint, previousPoint);
                }

                if (previousPoint != null)
                {
                    CalcDistanceAndSpeedStatistics(track, trackPoint, previousPoint, ref averageSpeedTrackPointCount);
                }

                previousPoint = trackPoint;
            }

            if (track.MaxHeight <= -MaxHeightLimit)
            {
                track.MaxHeight = 0.0;
            }

            if (track.MinHeight >= MaxHeightLimit)
            {
                track.MinHeight = 0.0;
            }

            if (averageSpeedTrackPointCount > 0)
            {
                track.AverageSpeed /= averageSpeedTrackPointCount;
            }
        }

        /// <summary>
        /// Resets statistics for given track
        /// </summary>
        /// <param name="track">track to reset</param>
        private static void ResetStatistics(this Track track)
        {
            track.Duration = TimeSpan.Zero;
            track.LengthInMeter = 0.0;
            track.HeightGain = 0.0;
            track.HeightLoss = 0.0;
            track.MaxHeight = -MaxHeightLimit;
            track.MinHeight = MaxHeightLimit;
            track.MaxSpeed = 0.0;
            track.AverageSpeed = 0.0;
            track.MaxClimbRate = 0.0;
            track.MaxSinkRate = 0.0;
        }

        /// <summary>
        /// Calculates track duration
        /// </summary>
        /// <param name="track">track to calculate duration for</param>
        private static void CalcTrackDuration(Track track)
        {
            var firstTrackPointWithTime = track.TrackPoints.FirstOrDefault(x => x.Time.HasValue);
            var lastTrackPointWithTime = track.TrackPoints.LastOrDefault(x => x.Time.HasValue);

            if (firstTrackPointWithTime != null &&
                firstTrackPointWithTime.Time != null &&
                lastTrackPointWithTime != null &&
                lastTrackPointWithTime.Time != null)
            {
                track.Duration = lastTrackPointWithTime.Time.Value - firstTrackPointWithTime.Time.Value;
            }
        }

        /// <summary>
        /// Calculates altitude based statistics
        /// </summary>
        /// <param name="track">track to calculate statistics for</param>
        /// <param name="trackPoint">current track point</param>
        /// <param name="previousPoint">previous track point</param>
        private static void CalcAltitudeStatistics(Track track, TrackPoint trackPoint, TrackPoint previousPoint)
        {
            if (!trackPoint.Altitude.HasValue)
            {
                return;
            }

            double altitude = trackPoint.Altitude.Value;

            track.MinHeight = Math.Min(altitude, track.MinHeight);
            track.MaxHeight = Math.Max(altitude, track.MaxHeight);

            double? previousHeight = previousPoint?.Altitude;

            double timeInSeconds = 1.0;
            if (previousPoint != null &&
                previousPoint.Time.HasValue &&
                trackPoint.Time.HasValue)
            {
                timeInSeconds = (trackPoint.Time.Value - previousPoint.Time.Value).TotalSeconds;
            }

            if (previousHeight.HasValue)
            {
                double altitudeDelta = altitude - previousHeight.Value;
                double climbRate = Math.Abs(timeInSeconds) < 1e-6 ? 0.0 : altitudeDelta / timeInSeconds;

                if (altitudeDelta > 0.0)
                {
                    track.HeightGain += altitudeDelta;
                    track.MaxClimbRate = Math.Max(track.MaxClimbRate, climbRate);
                }
                else if (altitudeDelta < 0.0)
                {
                    track.HeightLoss += -altitudeDelta;
                    track.MaxSinkRate = Math.Min(track.MaxSinkRate, climbRate);
                }
            }
        }

        /// <summary>
        /// Calculates distance and speed statistics
        /// </summary>
        /// <param name="track">track to update</param>
        /// <param name="trackPoint">current track point</param>
        /// <param name="previousPoint">previous track point</param>
        /// <param name="averageSpeedTrackPointCount">
        /// reference to current track point could for calculating average speed
        /// </param>
        private static void CalcDistanceAndSpeedStatistics(Track track, TrackPoint trackPoint, TrackPoint previousPoint, ref int averageSpeedTrackPointCount)
        {
            var point1 = new MapPoint(previousPoint.Latitude, previousPoint.Longitude);
            var point2 = new MapPoint(trackPoint.Latitude, trackPoint.Longitude);
            double distanceInMeter = point1.DistanceTo(point2);

            track.LengthInMeter += distanceInMeter;

            double timeInSeconds = 1.0;
            if (previousPoint.Time.HasValue && trackPoint.Time.HasValue)
            {
                timeInSeconds = (trackPoint.Time.Value - previousPoint.Time.Value).TotalSeconds;
            }

            double speedInKmh = distanceInMeter / timeInSeconds * Constants.FactorMeterPerSecondToKilometerPerHour;
            if (Math.Abs(timeInSeconds) < 1e-6)
            {
                speedInKmh = 0.0;
            }

            track.MaxSpeed = Math.Max(track.MaxSpeed, speedInKmh);

            track.AverageSpeed += speedInKmh;
            averageSpeedTrackPointCount++;
        }

        /// <summary>
        /// Calculates XC Tracer time points, starting with the time point in the track name and
        /// using a time interval of 0.2s.
        /// </summary>
        /// <param name="track">track to modify</param>
        public static void CalcXCTracerTimePoints(this Track track)
        {
            int pos = track.Name.IndexOf(' ');
            string startDateText = pos != -1 ? track.Name.Substring(0, pos) : string.Empty;

            if (!string.IsNullOrWhiteSpace(startDateText) &&
                DateTimeOffset.TryParse(startDateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset startDate))
            {
                track.GenerateTrackPointTimeValues(startDate, TimeSpan.FromSeconds(0.2));
            }
        }
    }
}
