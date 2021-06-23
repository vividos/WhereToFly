using System;
using System.Collections.Generic;

namespace WhereToFly.Geo.Model
{
    /// <summary>
    /// A single track consisting of track points
    /// </summary>
    public sealed class Track : IEquatable<Track>
    {
        /// <summary>
        /// ID for the track; e.g. generated at import time
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Track name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Track description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if track is a flight track and will be colored depending on climb and sink
        /// rates.
        /// </summary>
        public bool IsFlightTrack { get; set; } = false;

        /// <summary>
        /// Indicates if the track is a live track that is updated periodically
        /// </summary>
        public bool IsLiveTrack { get; set; }

        /// <summary>
        /// Color to use for the track; used when not a flight track; format is RRGGBB in hex.
        /// </summary>
        public string Color { get; set; } = "0000FF";

        /// <summary>
        /// Duration of track
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Length of track, in meter
        /// </summary>
        public double LengthInMeter { get; set; } = 0.0;

        /// <summary>
        /// Height gain, in meter
        /// </summary>
        public double HeightGain { get; set; } = 0.0;

        /// <summary>
        /// Height loss, in meter
        /// </summary>
        public double HeightLoss { get; set; } = 0.0;

        /// <summary>
        /// Max. height, in meter
        /// </summary>
        public double MaxHeight { get; set; } = 0.0;

        /// <summary>
        /// Min. height, in meter
        /// </summary>
        public double MinHeight { get; set; } = 0.0;

        /// <summary>
        /// Max. climb rate, in m/s
        /// </summary>
        public double MaxClimbRate { get; set; } = 0.0;

        /// <summary>
        /// Max. sink rate (or min. negative climb rate), in m/s
        /// </summary>
        public double MaxSinkRate { get; set; } = 0.0;

        /// <summary>
        /// Maximum speed, in km/h
        /// </summary>
        public double MaxSpeed { get; set; } = 0.0;

        /// <summary>
        /// Average speed, in km/h
        /// </summary>
        public double AverageSpeed { get; set; } = 0.0;

        /// <summary>
        /// List of track points
        /// </summary>
        public List<TrackPoint> TrackPoints { get; set; } = new List<TrackPoint>();

        /// <summary>
        /// List of altitude values, specifying the ground height profile; only set for flight
        /// tracks
        /// </summary>
        public List<double> GroundHeightProfile { get; set; } = new List<double>();

        /// <summary>
        /// Creates a new and empty track object
        /// </summary>
        public Track()
        {
        }

        #region IEquatable implementation

        /// <summary>
        /// Compares this track with another
        /// </summary>
        /// <param name="other">track to compare to</param>
        /// <returns>true when track are equal, false when not</returns>
        public bool Equals(Track other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Id == other.Id &&
                this.Name == other.Name &&
                this.Description == other.Description &&
                this.TrackPoints.Count == other.TrackPoints.Count &&
                this.GroundHeightProfile.Count == other.GroundHeightProfile.Count;
        }
        #endregion

        #region object overridables implementation

        /// <summary>
        /// Compares this track to another object
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true when tracks are equal, false when not</returns>
        public override bool Equals(object obj) =>
            (obj is Track track) && this.Equals(track);

        /// <summary>
        /// Calculates hash code for track
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 487;

            hashCode = (hashCode * 31) + this.Id.GetHashCode();
            hashCode = (hashCode * 31) + this.Name.GetHashCode();
            hashCode = (hashCode * 31) + this.Description.GetHashCode();
            hashCode = (hashCode * 31) + this.TrackPoints.GetHashCode();
            hashCode = (hashCode * 31) + this.GroundHeightProfile.GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Returns a printable representation of this object
        /// </summary>
        /// <returns>printable text</returns>
        public override string ToString()
        {
            return $"Name={this.Name}, TrackPoints.Count={this.TrackPoints.Count}";
        }
        #endregion
    }
}
