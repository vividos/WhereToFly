using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Services;
using WhereToFly.Geo.Model;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for track list page
    /// </summary>
    public class TrackListViewModel : ViewModelBase
    {
        /// <summary>
        /// Track list
        /// </summary>
        private List<Track> trackList = new();

        /// <summary>
        /// Backing field for "IsListRefreshActive" property
        /// </summary>
        private bool isListRefreshActive;

        #region Binding properties
        /// <summary>
        /// Current track list
        /// </summary>
        public ObservableCollection<TrackListEntryViewModel> TrackList { get; set; }

        /// <summary>
        /// Indicates if the refreshing of the track list is currently active, in order to show an
        /// activity indicator.
        /// </summary>
        public bool IsListRefreshActive
        {
            get => this.isListRefreshActive;
            private set
            {
                this.isListRefreshActive = value;
                this.OnPropertyChanged(nameof(this.IsListRefreshActive));
            }
        }

        /// <summary>
        /// Indicates if the track list is empty.
        /// </summary>
        public bool IsListEmpty
        {
            get => !this.isListRefreshActive &&
                (this.trackList == null || !this.trackList.Any());
        }

        /// <summary>
        /// Stores the selected track when an item is tapped
        /// </summary>
        public TrackListEntryViewModel SelectedTrack { get; set; }

        /// <summary>
        /// Indicates if the "delete track" button is enabled.
        /// </summary>
        public bool IsDeleteTrackEnabled
        {
            get => !this.isListRefreshActive &&
                (this.trackList != null && this.trackList.Any());
        }

        /// <summary>
        /// Command to execute when toolbar button "import track" has been tapped
        /// </summary>
        public ICommand ImportTrackCommand { get; private set; }

        /// <summary>
        /// Command to execute when toolbar button "delete track list" has been tapped
        /// </summary>
        public AsyncCommand DeleteTrackListCommand { get; private set; }
        #endregion

        /// <summary>
        /// Creates a new view model object for the track list
        /// </summary>
        public TrackListViewModel()
        {
            this.isListRefreshActive = false;

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            this.IsListRefreshActive = true;
            this.OnPropertyChanged(nameof(this.IsListEmpty));

            Task.Run(this.ReloadTrackListAsync);

            this.ImportTrackCommand = new AsyncCommand(this.ImportTrackAsync);

            this.DeleteTrackListCommand = new AsyncCommand(
                this.ClearTracksAsync,
                (obj) => this.IsDeleteTrackEnabled);
        }

        /// <summary>
        /// Loads data; async method
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadDataAsync()
        {
            try
            {
                IDataService dataService = DependencyService.Get<IDataService>();
                var trackDataService = dataService.GetTrackDataService();

                this.trackList = (await trackDataService.GetList()).ToList();
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }
        }

        /// <summary>
        /// Updates track list
        /// </summary>
        private void UpdateTrackList()
        {
            this.IsListRefreshActive = true;
            this.OnPropertyChanged(nameof(this.IsListEmpty));

            this.DeleteTrackListCommand.RaiseCanExecuteChanged();

            var newList = this.trackList
                .Select(track => new TrackListEntryViewModel(this, track));

            this.TrackList = new ObservableCollection<TrackListEntryViewModel>(newList);

            this.IsListRefreshActive = false;

            this.OnPropertyChanged(nameof(this.TrackList));
            this.OnPropertyChanged(nameof(this.IsListRefreshActive));
            this.OnPropertyChanged(nameof(this.IsListEmpty));

            this.DeleteTrackListCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Navigates to track info page, showing details about given track
        /// </summary>
        /// <param name="track">track to show</param>
        /// <returns>task to wait on</returns>
        internal async Task NavigateToTrackDetails(Track track)
        {
            this.SelectedTrack = null;
            this.OnPropertyChanged(nameof(this.SelectedTrack));

            await NavigationService.Instance.NavigateAsync(PageKey.TrackInfoPage, true, track);
        }

        /// <summary>
        /// Returns to map view and zooms to the given track
        /// </summary>
        /// <param name="track">track to zoom to</param>
        /// <returns>task to wait on</returns>
        internal async Task ZoomToTrack(Track track)
        {
            App.MapView.ZoomToTrack(track);

            await NavigationService.GoToMap();
        }

        /// <summary>
        /// Called when "Export" menu item is selected
        /// </summary>
        /// <param name="track">track to export</param>
        /// <returns>task to wait on</returns>
        internal async Task ExportTrack(Track track)
        {
            await ExportFileHelper.ExportTrackAsync(track);
        }

        /// <summary>
        /// Deletes the given track from the track list
        /// </summary>
        /// <param name="track">track to delete</param>
        /// <returns>task to wait on</returns>
        internal async Task DeleteTrack(Track track)
        {
            this.trackList.Remove(track);

            var dataService = DependencyService.Get<IDataService>();
            var trackDataService = dataService.GetTrackDataService();

            await trackDataService.Remove(track.Id);

            if (track.IsLiveTrack)
            {
                var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
                liveWaypointRefreshService.RemoveLiveTrack(track.Id);
            }

            this.UpdateTrackList();

            App.MapView.RemoveTrack(track);

            App.ShowToast("Selected track was deleted.");
        }

        /// <summary>
        /// Clears all tracks
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ClearTracksAsync()
        {
            bool result = await App.Current.MainPage.DisplayAlert(
                Constants.AppTitle,
                "Really clear all tracks?",
                "Clear",
                "Cancel");

            if (!result)
            {
                return;
            }

            var dataService = DependencyService.Get<IDataService>();
            var trackDataService = dataService.GetTrackDataService();

            await trackDataService.ClearList();

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.ClearLiveWaypointList();

            await this.ReloadTrackListAsync();

            App.MapView.ClearAllTracks();

            App.ShowToast("Track list was cleared.");
        }

        /// <summary>
        /// Reloads track list and shows it on the page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ReloadTrackListAsync()
        {
            await this.LoadDataAsync();
            await App.RunOnUiThreadAsync(this.UpdateTrackList);
        }

        /// <summary>
        /// Imports track from storage
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportTrackAsync()
        {
            bool success = false;

            try
            {
                var options = new Xamarin.Essentials.PickOptions
                {
                    FileTypes = new Xamarin.Essentials.FilePickerFileType(
                        new Dictionary<Xamarin.Essentials.DevicePlatform, IEnumerable<string>>
                        {
                            { Xamarin.Essentials.DevicePlatform.Android, null },
                            { Xamarin.Essentials.DevicePlatform.UWP, new string[] { ".kml", ".kmz", ".gpx", ".igc" } },
                            { Xamarin.Essentials.DevicePlatform.iOS, null },
                        }),
                    PickerTitle = "Select a Track file to import",
                };

                var result = await Xamarin.Essentials.FilePicker.PickAsync(options);
                if (result == null ||
                    string.IsNullOrEmpty(result.FullPath))
                {
                    return;
                }

                using var stream = await result.OpenReadAsync();
                success = await OpenFileHelper.OpenTrackAsync(stream, result.FileName);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while picking a file: " + ex.Message,
                    "OK");

                return;
            }

            if (success)
            {
                await NavigationService.Instance.GoBack();
            }
        }

        /// <summary>
        /// Checks if reload is needed, e.g. when track list has changed.
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task CheckReloadNeeded()
        {
            try
            {
                IDataService dataService = DependencyService.Get<IDataService>();
                var trackDataService = dataService.GetTrackDataService();

                var newTrackList = (await trackDataService.GetList()).ToList();

                if (this.trackList.Count != newTrackList.Count ||
                    this.TrackList == null ||
                    this.TrackList.Count != newTrackList.Count)
                {
                    this.trackList = newTrackList;

                    await App.RunOnUiThreadAsync(this.UpdateTrackList);
                }
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }
        }
    }
}
