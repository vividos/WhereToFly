using System;
using System.Collections.Generic;
using System.Linq;
using WhereToFly.App.Geo;
using WhereToFly.App.Geo.Spatial;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "add track" popup page
    /// </summary>
    public class AddTrackPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// Track being edited
        /// </summary>
        public Track Track { get; private set; }

        /// <summary>
        /// Track point interval as time span; default is 1.0 seconds
        /// </summary>
        private TimeSpan trackPointInterval = TimeSpan.FromSeconds(1.0);

        #region Binding properties
        /// <summary>
        /// Property containing the track name
        /// </summary>
        public string TrackName
        {
            get => this.Track.Name;
            set
            {
                this.Track.Name = value;
                this.OnPropertyChanged(nameof(this.TrackName));
            }
        }

        /// <summary>
        /// Property containing flag if the track point interval picker controls are visible
        /// </summary>
        public bool IsTrackPointIntervalPickerVisible { get; private set; }

        /// <summary>
        /// List of preset track point intervals
        /// </summary>
        public List<string> TrackPointIntervalList { get; private set; }

        /// <summary>
        /// Property that contains the track point interval, as text
        /// </summary>
        public string TrackPointIntervalText
        {
            get => $"{(int)this.trackPointInterval.TotalSeconds} s";
            set
            {
                if (double.TryParse(
                    value.Replace(" s", string.Empty),
                    System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double trackPointIntervalInSeconds))
                {
                    this.trackPointInterval = TimeSpan.FromSeconds(trackPointIntervalInSeconds);
                }
            }
        }

        /// <summary>
        /// Property containing flag if the track is a flight
        /// </summary>
        public bool IsFlightTrack
        {
            get => this.Track.IsFlightTrack;
            set
            {
                this.Track.IsFlightTrack = value;
                this.OnPropertyChanged(nameof(this.IsFlightTrack));
                this.OnPropertyChanged(nameof(this.IsColorPickerVisible));
            }
        }

        /// <summary>
        /// Property containing flag if the color picker controls are visible
        /// </summary>
        public bool IsColorPickerVisible
        {
            get => !this.Track.IsFlightTrack;
        }

        /// <summary>
        /// Propertiy containing the color of the track, in format RRGGBB
        /// </summary>
        public string SelectedTrackColor
        {
            get => this.Track.Color;
            set
            {
                this.Track.Color = value;
                this.OnPropertyChanged(nameof(this.SelectedTrackColor));
            }
        }
        #endregion

        /// <summary>
        /// Creates a new "add track" popup page view model
        /// </summary>
        /// <param name="track">track to edit</param>
        public AddTrackPopupViewModel(Track track)
        {
            this.Track = track;

            this.IsTrackPointIntervalPickerVisible = !track.TrackPoints.Any(trackPoint => trackPoint.Time.HasValue);

            this.TrackPointIntervalList = new List<string>
            {
                "1 s",
                "2 s",
                "3 s",
                "0.5 s",
                "0.2 s",
            };
        }

        /// <summary>
        /// Updates track, e.g. with new track point interval
        /// </summary>
        public void UpdateTrack()
        {
            if (this.IsTrackPointIntervalPickerVisible)
            {
                this.Track.GenerateTrackPointTimeValues(DateTimeOffset.Now, this.trackPointInterval);
                this.Track.CalculateStatistics();
            }
        }
    }
}
