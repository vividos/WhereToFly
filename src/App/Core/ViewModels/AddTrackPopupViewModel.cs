using System;
using System.Collections.Generic;
using System.Linq;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

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

        /// <summary>
        /// Track offset, in meter; default value is 0 meter
        /// </summary>
        private int trackOffset;

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
                this.OnPropertyChanged(nameof(this.IsTrackOffsetPickerVisible));
                this.OnPropertyChanged(nameof(this.TrackOffset));
                this.OnPropertyChanged(nameof(this.IsColorPickerVisible));
            }
        }

        /// <summary>
        /// Property containing flag if the track offset controls are visible
        /// </summary>
        public bool IsTrackOffsetPickerVisible
        {
            get => this.Track.IsFlightTrack;
        }

        /// <summary>
        /// Track offset as text
        /// </summary>
        public string TrackOffset
        {
            get => this.trackOffset.ToString();
            set
            {
                this.trackOffset = string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
                this.OnPropertyChanged(nameof(this.TrackOffset));
            }
        }

        /// <summary>
        /// Property containing flag if the color picker controls are visible
        /// </summary>
        public bool IsColorPickerVisible => !this.Track.IsFlightTrack;

        /// <summary>
        /// Property containing the color of the track
        /// </summary>
        public Color SelectedTrackColor
        {
            get => Color.FromHex("#" + this.Track.Color);
            set
            {
                this.Track.Color = value.ToHex().Replace("#FF", string.Empty);
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
        /// Updates track, e.g. with new track point interval or track offset
        /// </summary>
        public void UpdateTrack()
        {
            if (this.IsTrackPointIntervalPickerVisible)
            {
                this.Track.GenerateTrackPointTimeValues(DateTimeOffset.Now, this.trackPointInterval);
                this.Track.CalculateStatistics();
            }

            if (this.IsFlightTrack &&
                this.trackOffset != 0)
            {
                this.Track.ApplyAltitudeOffset(this.trackOffset);
            }
        }
    }
}
