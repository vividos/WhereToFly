using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Services;
using WhereToFly.Geo.Model;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the track details page
    /// </summary>
    public class TrackTabViewModel
    {
        /// <summary>
        /// Track to show
        /// </summary>
        private readonly Track track;

        #region Binding properties
        /// <summary>
        /// Command to execute when "zoom to" menu item is selected on a track
        /// </summary>
        public ICommand ZoomToTrackCommand { get; set; }

        /// <summary>
        /// Command to execute when "export" menu item is selected on a track
        /// </summary>
        public ICommand ExportTrackCommand { get; set; }

        /// <summary>
        /// Command to execute when "delete" menu item is selected on a track
        /// </summary>
        public ICommand DeleteTrackCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model object based on the given track object
        /// </summary>
        /// <param name="track">track object</param>
        public TrackTabViewModel(Track track)
        {
            this.track = track;

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.ZoomToTrackCommand = new AsyncCommand(this.OnZoomToTrackAsync);
            this.ExportTrackCommand = new AsyncCommand(this.OnExportTrackAsync);
            this.DeleteTrackCommand = new AsyncCommand(this.OnDeleteTrackAsync);
        }

        /// <summary>
        /// Called when "Zoom to" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToTrackAsync()
        {
            App.MapView.ZoomToTrack(this.track);

            await NavigationService.GoToMap();
        }

        /// <summary>
        /// Called when "Export" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnExportTrackAsync()
        {
            await ExportFileHelper.ExportTrackAsync(this.track);
        }

        /// <summary>
        /// Called when "Delete" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteTrackAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            var trackDataService = dataService.GetTrackDataService();

            await trackDataService.Remove(this.track.Id);

            App.MapView.RemoveTrack(this.track);

            await NavigationService.Instance.GoBack();

            App.ShowToast("Selected track was deleted.");
        }
    }
}
