using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Geo;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for track list page
    /// </summary>
    public class TrackListViewModel : ViewModelBase
    {
        /// <summary>
        /// Location list
        /// </summary>
        private List<Track> trackList = new List<Track>();

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
        /// Command to execute when toolbar button "import track" has been tapped
        /// </summary>
        public Command ImportTrackCommand { get; private set; }

        /// <summary>
        /// Command to execute when toolbar button "delete track list" has been tapped
        /// </summary>
        public Command DeleteTrackListCommand { get; private set; }

        /// <summary>
        /// Command to execute when an item in the track list has been tapped
        /// </summary>
        public Command<Track> ItemTappedCommand { get; private set; }
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
            Task.Run(this.LoadDataAsync);

            this.ImportTrackCommand =
                new Command(async () =>
                {
                    await this.ImportTrackAsync();
                });

            this.DeleteTrackListCommand =
                new Command(async () =>
                {
                    await this.ClearTracksAsync();
                });

            this.ItemTappedCommand =
                new Command<Track>(async (track) =>
                {
                    await this.NavigateToTrackDetails(track);
                });
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

                this.trackList = await dataService.GetTrackListAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }

            this.UpdateTrackList();
        }

        /// <summary>
        /// Updates track list
        /// </summary>
        private void UpdateTrackList()
        {
            this.IsListRefreshActive = true;

            var newList = this.trackList
                .Select(track => new TrackListEntryViewModel(this, track));

            this.TrackList = new ObservableCollection<TrackListEntryViewModel>(newList);

            this.OnPropertyChanged(nameof(this.TrackList));
            this.OnPropertyChanged(nameof(this.IsListEmpty));

            this.IsListRefreshActive = false;
        }

        /// <summary>
        /// Navigates to track info page, showing details about given track
        /// </summary>
        /// <param name="track">track to show</param>
        /// <returns>task to wait on</returns>
        internal async Task NavigateToTrackDetails(Track track)
        {
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyTrackDetailsPage, true, track);
        }

        /// <summary>
        /// Returns to map view and zooms to the given location
        /// </summary>
        /// <param name="track">location to zoom to</param>
        /// <returns>task to wait on</returns>
        internal async Task ZoomToTrack(Track track)
        {
            App.ZoomToTrack(track);

            await NavigationService.Instance.NavigateAsync(Constants.PageKeyMapPage, animated: true);
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
            await dataService.StoreTrackListAsync(this.trackList);

            this.UpdateTrackList();

            App.UpdateMapTracksList();

            App.ShowToast("Selected track was deleted.");
        }

        /// <summary>
        /// Clears all locations
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
            await dataService.StoreTrackListAsync(new List<Track>());

            await this.ReloadTrackListAsync();

            App.UpdateMapTracksList();

            App.ShowToast("Track list was cleared.");
        }

        /// <summary>
        /// Reloads track list and shows it on the page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ReloadTrackListAsync()
        {
            await this.LoadDataAsync();
            this.UpdateTrackList();
        }

        /// <summary>
        /// Imports track from storage
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportTrackAsync()
        {
            FileData result = null;
            try
            {
                result = await CrossFilePicker.Current.PickFile();
                if (result == null ||
                    string.IsNullOrEmpty(result.FilePath))
                {
                    return;
                }
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

            using (var stream = result.GetStream())
            {
                await OpenFileHelper.OpenTrackAsync(stream, result.FileName);
            }

            await NavigationService.Instance.GoBack();
        }
    }
}
