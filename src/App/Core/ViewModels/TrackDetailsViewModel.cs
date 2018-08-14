using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Geo;
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
