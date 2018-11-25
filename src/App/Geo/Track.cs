using System;
using System.Collections.Generic;

namespace WhereToFly.App.Geo
{
    /// <summary>
    /// A single track consisting of track points
    /// </summary>
    public class Track
    {
        /// <summary>
        /// ID for the track; e.g. generated at import time
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Track name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates if track is a flight track and will be colored depending on climb and sink
        /// rates.
        /// </summary>
        public bool IsFlightTrack { get; set; }

        /// <summary>
        /// Color to use for the track; used when not a flight track; format is RRGGBB in hex.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Duration of track
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Length of track, in meter
        /// </summary>
        public double LengthInMeter { get; set; }

        /// <summary>
        /// Height gain, in meter
        /// </summary>
        public double HeightGain { get; set; }

        /// <summary>
        /// Height gain, in meter
        /// </summary>
        public double HeightLoss { get; set; }

        /// <summary>
        /// Max. height, in meter
        /// </summary>
        public double MaxHeight { get; set; }

        /// <summary>
        /// Min. height, in meter
        /// </summary>
        public double MinHeight { get; set; }

        /// <summary>
        /// Max. climb rate, in m/s
        /// </summary>
        public double MaxClimbRate { get; set; }

        /// <summary>
        /// Max. sink rate (or min. negative climb rate), in m/s
        /// </summary>
        public double MaxSinkRate { get; set; }

        /// <summary>
        /// Maximum speed, in km/h
        /// </summary>
        public double MaxSpeed { get; set; }

        /// <summary>
        /// Average speed, in km/h
        /// </summary>
        public double AverageSpeed { get; set; }

        /// <summary>
        /// List of track points
        /// </summary>
        public List<TrackPoint> TrackPoints { get; set; }

        /// <summary>
        /// Creates a new and empty track object
        /// </summary>
        public Track()
        {
            this.Name = string.Empty;
            this.IsFlightTrack = false;
            this.Color = "0000FF";
            this.Duration = TimeSpan.Zero;
            this.LengthInMeter = 0.0;
            this.HeightGain = 0.0;
            this.HeightLoss = 0.0;
            this.MaxHeight = 0.0;
            this.MinHeight = 0.0;
            this.MaxSpeed = 0.0;
            this.AverageSpeed = 0.0;
            this.MaxClimbRate = 0.0;
            this.MaxSinkRate = 0.0;
            this.TrackPoints = new List<TrackPoint>();
        }
    }
}
