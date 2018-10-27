using WhereToFly.App.Geo;

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
        /// Property containing flag if the track is a flight
        /// </summary>
        public bool IsFlightTrack
        {
            get => this.Track.IsFlightTrack;
            set
            {
                this.Track.IsFlightTrack = value;
                this.OnPropertyChanged(nameof(this.IsFlightTrack));
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
        }
    }
}
