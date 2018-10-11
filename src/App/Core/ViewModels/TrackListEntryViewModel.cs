using System;
using System.IO;
using System.Threading.Tasks;
using WhereToFly.App.Geo;
using WhereToFly.App.Logic;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for a single track list entry
    /// </summary>
    public class TrackListEntryViewModel
    {
        /// <summary>
        /// Parent view model
        /// </summary>
        private readonly TrackListViewModel parentViewModel;

        /// <summary>
        /// Track to show
        /// </summary>
        private readonly Track track;

        /// <summary>
        /// Lazy-loading backing store for type image source
        /// </summary>
        private readonly Lazy<ImageSource> typeImageSource;

        /// <summary>
        /// Property containing the track object
        /// </summary>
        public Track Track => this.track;

        /// <summary>
        /// Property containing location name
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
        public ImageSource TypeImageSource
        {
            get
            {
                return this.typeImageSource.Value;
            }
        }

        /// <summary>
        /// Property containing detail infos for track
        /// </summary>
        public string DetailInfos
        {
            get
            {
                return string.Format(
                    "Duration: {0}, Length: {1}, Max. Climb {2:F1} m/s",
                    DataFormatter.FormatDuration(this.track.Duration),
                    DataFormatter.FormatDistance(this.track.LengthInMeter),
                    this.track.MaxClimbRate);
            }
        }

        /// <summary>
        /// Command to execute when "show details" context action is selected on a track
        /// </summary>
        public Command ShowTrackDetailsContextAction { get; set; }

        /// <summary>
        /// Command to execute when "zoom to" context action is selected on a track
        /// </summary>
        public Command ZoomToTrackContextAction { get; set; }

        /// <summary>
        /// Command to execute when "delete" context action is selected on a location
        /// </summary>
        public Command DeleteTrackContextAction { get; set; }

        /// <summary>
        /// Creates a new view model object based on the given track object
        /// </summary>
        /// <param name="parentViewModel">parent view model</param>
        /// <param name="track">track object</param>
        public TrackListEntryViewModel(TrackListViewModel parentViewModel, Track track)
        {
            this.parentViewModel = parentViewModel;
            this.track = track;

            this.typeImageSource = new Lazy<ImageSource>(this.GetTypeImageSource);

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.ShowTrackDetailsContextAction =
                new Command(async () => await this.OnShowDetailsLocation());

            this.ZoomToTrackContextAction =
                new Command(async () => await this.OnZoomToTrackAsync());

            this.DeleteTrackContextAction =
                new Command(async () => await this.OnDeleteTrackAsync());
        }

        /// <summary>
        /// Called when "show details" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnShowDetailsLocation()
        {
            await this.parentViewModel.NavigateToTrackDetails(this.track);
        }

        /// <summary>
        /// Called when "zoom to" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToTrackAsync()
        {
            await this.parentViewModel.ZoomToTrack(this.track);
        }

        /// <summary>
        /// Called when "delete" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteTrackAsync()
        {
            await this.parentViewModel.DeleteTrack(this.track);
        }

        /// <summary>
        /// Returns type icon from track type
        /// </summary>
        /// <returns>image source, or null when no icon could be found</returns>
        private ImageSource GetTypeImageSource()
        {
            string svgImagePath = this.track.IsFlightTrack ? "map/images/paragliding.svg" : "icons/map-marker-distance.svg";

            string svgText = DependencyService.Get<SvgImageCache>()
                .GetSvgImage(svgImagePath, "#000000");

            if (svgText != null)
            {
                return ImageSource.FromStream(() => new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgText)));
            }

            return null;
        }
    }
}
