using System;
using System.Linq;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Geo.Spatial
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
                lastTrackPointWithTime != null)
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
    }
}
