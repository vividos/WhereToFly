using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "set track infos" popup page
    /// </summary>
    public class SetTrackInfoPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// Track being edited
        /// </summary>
        public Track Track { get; set; }

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
        /// Creates a new "set track info" popup page view model
        /// </summary>
        /// <param name="track">track to edit</param>
        public SetTrackInfoPopupViewModel(Track track)
        {
            this.Track = track;
        }
    }
}
