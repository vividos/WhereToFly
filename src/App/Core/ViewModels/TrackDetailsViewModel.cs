using WhereToFly.App.Geo;
using WhereToFly.App.Logic;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the track details page
    /// </summary>
    public class TrackDetailsViewModel
    {
        /// <summary>
        /// Track to show
        /// </summary>
        private readonly Track track;

        #region Binding properties
        /// <summary>
        /// Property containing track name
        /// </summary>
        public string Name
        {
            get
            {
                return this.track.Name;
            }
        }

        /// <summary>
        /// Returns image source for SvgImage in order to display the type image
        /// </summary>
        public ImageSource TypeImageSource { get; }

        /// <summary>
        /// Property that specifies if the color box is visible
        /// </summary>
        public bool IsColorBoxVisible => !this.track.IsFlightTrack;

        /// <summary>
        /// Property that contains the track's color
        /// </summary>
        public Color TrackColor => this.track.IsFlightTrack ? Color.Transparent : Color.FromHex(this.track.Color);

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
        #endregion

        /// <summary>
        /// Creates a new view model object based on the given track object
        /// </summary>
        /// <param name="track">track object</param>
        public TrackDetailsViewModel(Track track)
        {
            this.track = track;

            this.TypeImageSource = SvgImageCache.GetImageSource(track, "#000000");
        }
    }
}
