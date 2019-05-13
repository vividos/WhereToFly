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

            this.TypeImageSource = SvgImageCache.GetImageSource(track, "#000000");

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
    }
}
