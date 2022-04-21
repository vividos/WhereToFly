using WhereToFly.App.Core.Logic;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the track statistics page
    /// </summary>
    public class TrackStatisticsViewModel
    {
        /// <summary>
        /// Track to show
        /// </summary>
        private readonly Track track;

        #region Binding properties
        /// <summary>
        /// Property containing distance
        /// </summary>
        public string Distance
        {
            get
            {
                return DataFormatter.FormatDistance(this.track.LengthInMeter);
            }
        }

        /// <summary>
        /// Property containing duration
        /// </summary>
        public string Duration
        {
            get
            {
                return DataFormatter.FormatDuration(this.track.Duration);
            }
        }

        /// <summary>
        /// Property containing number of track points
        /// </summary>
        public int NumTrackPoints
        {
            get
            {
                return this.track.TrackPoints.Count;
            }
        }

        /// <summary>
        /// Property containing height gain
        /// </summary>
        public string HeightGain
        {
            get
            {
                return this.track.HeightGain.ToString("F1") + " m";
            }
        }

        /// <summary>
        /// Property containing height loss
        /// </summary>
        public string HeightLoss
        {
            get
            {
                return this.track.HeightLoss.ToString("F1") + " m";
            }
        }

        /// <summary>
        /// Property containing maximum height
        /// </summary>
        public string MaxHeight
        {
            get
            {
                return this.track.MaxHeight.ToString("F1") + " m";
            }
        }

        /// <summary>
        /// Property containing minimum height
        /// </summary>
        public string MinHeight
        {
            get
            {
                return this.track.MinHeight.ToString("F1") + " m";
            }
        }

        /// <summary>
        /// Property containing max. climb rate
        /// </summary>
        public string MaxClimbRate
        {
            get
            {
                return this.track.MaxClimbRate.ToString("F1") + " m/s";
            }
        }

        /// <summary>
        /// Property containing max. sink rate
        /// </summary>
        public string MaxSinkRate
        {
            get
            {
                return this.track.MaxSinkRate.ToString("F1") + " m/s";
            }
        }

        /// <summary>
        /// Property containing max. speed
        /// </summary>
        public string MaxSpeed
        {
            get
            {
                return this.track.MaxSpeed.ToString("F1") + " km/h";
            }
        }

        /// <summary>
        /// Property containing average speed
        /// </summary>
        public string AverageSpeed
        {
            get
            {
                return this.track.AverageSpeed.ToString("F1") + " km/h";
            }
        }
        #endregion

        /// <summary>
        /// Creates a new view model object based on the given track object
        /// </summary>
        /// <param name="track">track object</param>
        public TrackStatisticsViewModel(Track track) => this.track = track;
    }
}
