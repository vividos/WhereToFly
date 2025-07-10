using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using WhereToFly.App.Logic;
using WhereToFly.App.Popups;
using WhereToFly.App.Services;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the "plan tour" popup page
    /// </summary>
    public class PlanTourPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// Tour planning parameters
        /// </summary>
        private readonly PlanTourParameters planTourParameters;

        /// <summary>
        /// Function to call to close popup page
        /// </summary>
        private readonly Func<Task> closePopupPage;

        #region Binding properties
        /// <summary>
        /// List of locations to plan a tour for
        /// </summary>
        public ObservableCollection<PlanTourListEntryViewModel> PlanTourList { get; private set; } = [];

        /// <summary>
        /// Command to start tour planning
        /// </summary>
        public AsyncRelayCommand PlanTourCommand { get; set; }

        /// <summary>
        /// Command to close popup page
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// Property that returns if tour planning is possible
        /// </summary>
        public bool IsTourPlanningPossible { get => this.PlanTourList.Count > 1; }

        /// <summary>
        /// Property that returns true when a warning should be shown that more locations have to
        /// be added for tour planning.
        /// </summary>
        public bool ShowWarningForMoreLocations { get => !this.IsTourPlanningPossible; }
        #endregion

        /// <summary>
        /// Creates a view model for the "plan tour" popup page
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <param name="closePopupPage">function to call to close popup page</param>
        public PlanTourPopupViewModel(PlanTourParameters planTourParameters, Func<Task> closePopupPage)
        {
            this.planTourParameters = planTourParameters;
            this.closePopupPage = closePopupPage;

            this.PlanTourCommand = new AsyncRelayCommand(
                this.PlanTourAsync,
                () => this.IsTourPlanningPossible);

            this.CloseCommand = new AsyncRelayCommand(this.ClosePageAsync);

            MainThread.BeginInvokeOnMainThread(
                async () => await this.LoadDataAsync(
                    planTourParameters.WaypointIdList));
        }

        /// <summary>
        /// Loads data for tour planning
        /// </summary>
        /// <param name="waypointIdList">list of waypoint IDs</param>
        /// <returns>task to wait for</returns>
        private async Task LoadDataAsync(List<string> waypointIdList)
        {
            var dataService = DependencyService.Get<IDataService>();
            var locationDataService = dataService.GetLocationDataService();

            var locationList = await locationDataService.GetList();

            var viewModelList =
                from waypointId in waypointIdList
                let location = locationList.FirstOrDefault(locationToCheck => locationToCheck.Id == waypointId)
                where location != null
                select new PlanTourListEntryViewModel(location, this);

            this.PlanTourList = new ObservableCollection<PlanTourListEntryViewModel>(viewModelList);
            this.OnPropertyChanged(nameof(this.PlanTourList));
            this.OnPropertyChanged(nameof(this.ShowWarningForMoreLocations));
            this.PlanTourCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Returns if given location is the first location in the list
        /// </summary>
        /// <param name="viewModel">location view model to check</param>
        /// <returns>true when it's the first location, false when not</returns>
        internal bool IsFirstLocation(PlanTourListEntryViewModel viewModel)
        {
            return this.PlanTourList.FirstOrDefault()?.Id == viewModel.Id;
        }

        /// <summary>
        /// Returns if given location is the last location in the list
        /// </summary>
        /// <param name="viewModel">location view model to check</param>
        /// <returns>true when it's the last location, false when not</returns>
        internal bool IsLastLocation(PlanTourListEntryViewModel viewModel)
        {
            return this.PlanTourList.LastOrDefault()?.Id == viewModel.Id;
        }

        /// <summary>
        /// Moves location down one entry in the list
        /// </summary>
        /// <param name="viewModel">location view model to move</param>
        internal void MoveDownLocation(PlanTourListEntryViewModel viewModel)
        {
            Debug.Assert(!this.IsLastLocation(viewModel), "must not be called with the last location");

            var locationViewModel = this.PlanTourList.FirstOrDefault(viewModelToCheck => viewModelToCheck.Id == viewModel.Id);
            if (locationViewModel != null)
            {
                int index = this.PlanTourList.IndexOf(locationViewModel);
                Debug.Assert(index + 1 < this.PlanTourList.Count, "invalid index for moving location");

                this.PlanTourList.Move(index, index + 1);
                this.OnPropertyChanged(nameof(this.PlanTourList));

                foreach (PlanTourListEntryViewModel entryViewModel in this.PlanTourList)
                {
                    entryViewModel.Update();
                }

                this.UpdatePlanTourParameters();
            }
        }

        /// <summary>
        /// Moves location up one entry in the list
        /// </summary>
        /// <param name="viewModel">location view model to move</param>
        internal void MoveUpLocation(PlanTourListEntryViewModel viewModel)
        {
            Debug.Assert(!this.IsFirstLocation(viewModel), "must not be called with the first location");

            var locationViewModel = this.PlanTourList.FirstOrDefault(viewModelToCheck => viewModelToCheck.Id == viewModel.Id);
            if (locationViewModel != null)
            {
                int index = this.PlanTourList.IndexOf(locationViewModel);
                Debug.Assert(index > 0, "invalid index for moving location");

                this.PlanTourList.Move(index, index - 1);
                this.OnPropertyChanged(nameof(this.PlanTourList));

                foreach (PlanTourListEntryViewModel entryViewModel in this.PlanTourList)
                {
                    entryViewModel.Update();
                }

                this.UpdatePlanTourParameters();
            }
        }

        /// <summary>
        /// Removes location from list
        /// </summary>
        /// <param name="viewModel">location view model to remove</param>
        internal void RemoveLocation(PlanTourListEntryViewModel viewModel)
        {
            var locationViewModel = this.PlanTourList.FirstOrDefault(
                viewModelToCheck => viewModelToCheck.Id == viewModel.Id);

            if (locationViewModel == null)
            {
                return;
            }

            this.PlanTourList.Remove(locationViewModel);
            this.OnPropertyChanged(nameof(this.PlanTourList));
            this.OnPropertyChanged(nameof(this.ShowWarningForMoreLocations));

            this.UpdatePlanTourParameters();

            foreach (PlanTourListEntryViewModel entryViewModel in this.PlanTourList)
            {
                entryViewModel.Update();
            }

            this.PlanTourCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Updates tour planning parameters from binding properties
        /// </summary>
        private void UpdatePlanTourParameters()
        {
            this.planTourParameters.WaypointIdList =
                (from location in this.PlanTourList
                 select location.Location.Id)
                 .ToList();

            bool hasAnyNonPlanTourLocation =
                this.PlanTourList
                .Any(location => !location.Location.IsPlanTourLocation);

            if (hasAnyNonPlanTourLocation)
            {
                this.planTourParameters.WaypointLocationList =
                    this.PlanTourList
                    .Select(location =>
                        new Location(
                            location.Location.Id,
                            location.Location.MapLocation)
                        {
                            IsTempPlanTourLocation = location.Location.IsTempPlanTourLocation,
                        })
                    .ToList();
            }
            else
            {
                this.planTourParameters.WaypointLocationList.Clear();
            }
        }

        /// <summary>
        /// Closes popup page
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ClosePageAsync()
        {
            await this.closePopupPage.Invoke();
        }

        /// <summary>
        /// Plans tour with the current tour planning parameters
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task PlanTourAsync()
        {
            this.UpdatePlanTourParameters();

            await this.ClosePageAsync();

            PlannedTour? plannedTour = await this.CalculateTourAsync();
            if (plannedTour == null)
            {
                return;
            }

            var track = TrackFromPlannedTour(plannedTour);

            track = await NavigationService.Instance.NavigateToPopupPageAsync<Track?>(
                PopupPageKey.SetTrackInfosPopupPage,
                true,
                track);

            if (track == null)
            {
                return; // user canceled editing track properties
            }

            // sample track heights if the height profile has some missing altitude values
            if (track.TrackPoints.Any(trackPoint => !trackPoint.Altitude.HasValue))
            {
                await OpenFileHelper.AdjustTrackHeightsAsync(track);
            }

            track.CalculateStatistics();

            await AddTrack(track);

            var appMapService = DependencyService.Get<IAppMapService>();

            var location = StartWaypointFromPlannedTour(plannedTour);
            if (location != null)
            {
                await this.AddLocation(location);

                appMapService.MapView.AddLocation(location);
            }

            await appMapService.ClearTempPlanTourLocations();

            this.planTourParameters.WaypointIdList.Clear();
            this.planTourParameters.WaypointLocationList.Clear();

            await NavigationService.GoToMap();

            this.ShowTrack(track);
        }

        /// <summary>
        /// Calculates tour by calling backend. Shows waiting dialog while planning and shows
        /// error message when failed.
        /// </summary>
        /// <returns>planned tour, or null when tour couldn't be planned</returns>
        private async Task<PlannedTour?> CalculateTourAsync()
        {
            bool retry;
            do
            {
                retry = false;

                var waitingDialog = new WaitingPopupPage("Planning tour...");

                try
                {
                    waitingDialog.Show();

                    var dataService = DependencyService.Get<IDataService>();
                    return await dataService.PlanTourAsync(this.planTourParameters);
                }
                catch (Exception ex)
                {
                    App.LogError(ex);

                    retry = await UserInterface.DisplayAlert(
                        "Error while planning tour: " + ex.Message,
                        "Retry",
                        "Close");
                }
                finally
                {
                    await waitingDialog.HideAsync();
                }
            }
            while (retry);

            return null;
        }

        /// <summary>
        /// Creates track from planned tour
        /// </summary>
        /// <param name="plannedTour">planned tour</param>
        /// <returns>created track</returns>
        private static Track TrackFromPlannedTour(PlannedTour plannedTour)
        {
            var trackPoints = from mapPoint in plannedTour.MapPointList
                              select new TrackPoint(mapPoint.Latitude, mapPoint.Longitude, mapPoint.Altitude, null);

            var track = new Track(Guid.NewGuid().ToString("B"))
            {
                Name = "Planned Tour",
                Description = plannedTour.Description,
                IsFlightTrack = false,
                TrackPoints = trackPoints.ToList(),
                Attribution = plannedTour.Attribution,
            };

            track.CalculateStatistics();

            return track;
        }

        /// <summary>
        /// Adds track to data service
        /// </summary>
        /// <param name="track">track to add</param>
        /// <returns>task to wait on</returns>
        private static async Task AddTrack(Track track)
        {
            var dataService = DependencyService.Get<IDataService>();
            var trackDataService = dataService.GetTrackDataService();

            await trackDataService.Add(track);
        }

        /// <summary>
        /// Shows track on map
        /// </summary>
        /// <param name="track">track to show</param>
        private void ShowTrack(Track track)
        {
            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.MapView.AddTrack(track);
            appMapService.MapView.ZoomToTrack(track);

            UserInterface.DisplayToast("Track was added.");
        }

        /// <summary>
        /// Creates a starting waypoint from planned tour, including description
        /// </summary>
        /// <param name="plannedTour">planned tour</param>
        /// <returns>
        /// newly created start location, or null when the tour contains no map points
        /// </returns>
        private static Location? StartWaypointFromPlannedTour(PlannedTour plannedTour)
        {
            var startPoint = plannedTour.MapPointList.FirstOrDefault();
            if (startPoint == null)
            {
                return null;
            }

            return new Location(
                Guid.NewGuid().ToString("B"),
                startPoint)
            {
                Name = "Planned tour start",
                Description = plannedTour.Description,
                Type = LocationType.Waypoint,
                InternetLink = string.Empty,
            };
        }

        /// <summary>
        /// Adds location to the location list
        /// </summary>
        /// <param name="location">location to add</param>
        /// <returns>task to wait on</returns>
        private async Task AddLocation(Location location)
        {
            var dataService = DependencyService.Get<IDataService>();
            var locationDataService = dataService.GetLocationDataService();

            await locationDataService.Add(location);
        }
    }
}
