using System.Windows.Input;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Logic;
using WhereToFly.App.Services;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the track details page
    /// </summary>
    public class TrackTabViewModel : ViewModelBase
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

            this.ZoomToTrackCommand = new AsyncRelayCommand(this.OnZoomToTrackAsync);
            this.ExportTrackCommand = new AsyncRelayCommand(this.OnExportTrackAsync);
            this.DeleteTrackCommand = new AsyncRelayCommand(this.OnDeleteTrackAsync);
        }

        /// <summary>
        /// Called when "Zoom to" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToTrackAsync()
        {
            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.MapView.ZoomToTrack(this.track);

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

            try
            {
                await trackDataService.Remove(this.track.Id);
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }

            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.MapView.RemoveTrack(this.track);

            await NavigationService.Instance.GoBack();

            UserInterface.DisplayToast("Selected track was deleted.");
        }
    }
}
