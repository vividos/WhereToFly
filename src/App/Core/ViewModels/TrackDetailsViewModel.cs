using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
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

        /// <summary>
        /// Lazy-loading backing store for type image source
        /// </summary>
        private readonly Lazy<ImageSource> typeImageSource;

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
        public ImageSource TypeImageSource
        {
            get
            {
                return this.typeImageSource.Value;
            }
        }

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
        /// Property containing max. climb rate
        /// </summary>
        public string MaxClimbRate
        {
            get
            {
                return this.track.MaxClimbRate.ToString("F1");
            }
        }

        /// <summary>
        /// Command to execute when "zoom to" menu item is selected on a track
        /// </summary>
        public Command ZoomToTrackCommand { get; set; }

        /// <summary>
        /// Command to execute when "delete" menu item is selected on a track
        /// </summary>
        public Command DeleteTrackCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model object based on the given track object
        /// </summary>
        /// <param name="track">track object</param>
        public TrackDetailsViewModel(Track track)
        {
            this.track = track;

            this.typeImageSource = new Lazy<ImageSource>(this.GetTypeImageSource);

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.ZoomToTrackCommand =
                new Command(async () => await this.OnZoomToTrackAsync());

            this.DeleteTrackCommand =
                new Command(async () => await this.OnDeleteTrackAsync());
        }

        /// <summary>
        /// Called when "Zoom to" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToTrackAsync()
        {
            App.ZoomToTrack(this.track);

            await NavigationService.Instance.NavigateAsync(Constants.PageKeyMapPage, animated: true);
        }

        /// <summary>
        /// Called when "Delete" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteTrackAsync()
        {
            var dataService = DependencyService.Get<IDataService>();

            var trackList = await dataService.GetTrackListAsync(CancellationToken.None);

            trackList.RemoveAll(x => x.Id == this.track.Id);

            await dataService.StoreTrackListAsync(trackList);

            App.UpdateMapTracksList();

            await NavigationService.Instance.GoBack();

            App.ShowToast("Selected track was deleted.");
        }

        /// <summary>
        /// Returns type icon from location type
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
