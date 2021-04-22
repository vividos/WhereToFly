using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Logic;
using WhereToFly.Geo.Model;
using Xamarin.CommunityToolkit.ObjectModel;
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
        public ICommand ShowTrackDetailsContextAction { get; set; }

        /// <summary>
        /// Command to execute when "zoom to" context action is selected on a track
        /// </summary>
        public ICommand ZoomToTrackContextAction { get; set; }

        /// <summary>
        /// Command to execute when "Export" context action is selected on a track
        /// </summary>
        public ICommand ExportTrackContextAction { get; set; }

        /// <summary>
        /// Command to execute when "delete" context action is selected on a location
        /// </summary>
        public ICommand DeleteTrackContextAction { get; set; }

        /// <summary>
        /// Creates a new view model object based on the given track object
        /// </summary>
        /// <param name="parentViewModel">parent view model</param>
        /// <param name="track">track object</param>
        public TrackListEntryViewModel(TrackListViewModel parentViewModel, Track track)
        {
            this.parentViewModel = parentViewModel;
            this.track = track;

            this.TypeImageSource = SvgImageCache.GetImageSource(track);

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.ShowTrackDetailsContextAction = new AsyncCommand(this.OnShowDetailsLocation);
            this.ZoomToTrackContextAction = new AsyncCommand(this.OnZoomToTrackAsync);
            this.ExportTrackContextAction = new AsyncCommand(this.OnExportTrackAsync);
            this.DeleteTrackContextAction = new AsyncCommand(this.OnDeleteTrackAsync);
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
        /// Called when "Export" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnExportTrackAsync()
        {
            await this.parentViewModel.ExportTrack(this.track);
        }

        /// <summary>
        /// Called when "delete" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteTrackAsync()
        {
            await this.parentViewModel.DeleteTrack(this.track);
        }
    }
}
