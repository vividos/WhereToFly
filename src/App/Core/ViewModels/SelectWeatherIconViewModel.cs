using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Model;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the weather icon selection page. Used by SelectWeatherIconPage and
    /// SelectWeatherIconPopupPage.
    /// </summary>
    public partial class SelectWeatherIconViewModel : ViewModelBase
    {
        #region Binding properties
        /// <summary>
        /// Weather icon view models list
        /// </summary>
        public ObservableCollection<WeatherIconListEntryViewModel> WeatherIconList { get; set; }

        /// <summary>
        /// List of grouped weather icon view models
        /// </summary>
        public ObservableCollection<Grouping<string, WeatherIconListEntryViewModel>> GroupedWeatherIconList { get; set; }

        /// <summary>
        /// Stores the selected weather icon when an item is tapped
        /// </summary>
        public WeatherIconListEntryViewModel SelectedWeatherIcon { get; set; }

        /// <summary>
        /// Command to execute when an item in the weather icon list has been tapped
        /// </summary>
        public ICommand ItemTappedCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new select weather icon view model
        /// </summary>
        /// <param name="tcs">
        /// task completion source to use when a weather icon description was selected
        /// </param>
        /// <param name="group">group to filter by, or null to show all groups</param>
        public SelectWeatherIconViewModel(
            TaskCompletionSource<WeatherIconDescription> tcs,
            string group = null)
        {
            Debug.Assert(tcs != null, "task completion source must not be null");

            this.SetupBindings(tcs, group);
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        /// <param name="tcs">
        /// task completion source to use when a weather icon description was selected
        /// </param>
        /// <param name="group">group to filter by, or null to show all groups</param>
        private void SetupBindings(TaskCompletionSource<WeatherIconDescription> tcs, string group)
        {
            this.ItemTappedCommand = new Command<WeatherIconListEntryViewModel>(
                (itemViewModel) =>
                {
                    tcs.SetResult(itemViewModel.IconDescription);

                    this.SelectedWeatherIcon = null;
                    this.OnPropertyChanged(nameof(this.SelectedWeatherIcon));
                });

            Task.Run(async () => await this.LoadData(group));
        }

        /// <summary>
        /// Loads data for view model
        /// </summary>
        /// <param name="group">group to filter by, or null to show all groups</param>
        /// <returns>task to wait on</returns>
        private async Task LoadData(string group)
        {
            var dataService = DependencyService.Get<IDataService>();

            var weatherIconDescriptionDataService = dataService.GetWeatherIconDescriptionDataService();
            var weatherIconList = await weatherIconDescriptionDataService.GetList();

#pragma warning disable S1854 // Unused assignments should be removed
            var appManager = DependencyService.Get<IAppManager>();
#pragma warning restore S1854 // Unused assignments should be removed

            this.WeatherIconList = new ObservableCollection<WeatherIconListEntryViewModel>(
                from weatherIcon in weatherIconList
                where IsInGroup(@group, weatherIcon) && IsAppAvailable(weatherIcon)
                select new WeatherIconListEntryViewModel(this, weatherIcon));

            this.OnPropertyChanged(nameof(this.WeatherIconList));

            var groupedWeatherIconList =
                from weatherIconViewModel in this.WeatherIconList
                orderby GroupKeyFromGroup(weatherIconViewModel.Group)
                group weatherIconViewModel by weatherIconViewModel.Group into weatherIconGroup
                select new Grouping<string, WeatherIconListEntryViewModel>(
                    weatherIconGroup.Key, weatherIconGroup);

            this.GroupedWeatherIconList =
                new ObservableCollection<Grouping<string, WeatherIconListEntryViewModel>>(
                    groupedWeatherIconList);

            this.OnPropertyChanged(nameof(this.GroupedWeatherIconList));

            bool IsInGroup(string groupToCheck, WeatherIconDescription weatherIcon)
            {
                return groupToCheck == null || groupToCheck == weatherIcon.Group;
            }

            bool IsAppAvailable(WeatherIconDescription weatherIcon)
            {
                return weatherIcon.Type != WeatherIconDescription.IconType.IconApp ||
                    appManager.IsAvailable(weatherIcon.WebLink);
            }
        }

        /// <summary>
        /// Returns a group key for the group text; this is used to order weather icons by group.
        /// </summary>
        /// <param name="group">group text</param>
        /// <returns>group key; an order integer</returns>
        private static int GroupKeyFromGroup(string group)
        {
            switch (group)
            {
                case "Weather forecast": return 0;
                case "Current weather": return 1;
                case "Webcams": return 2;
                case "Android app": return 3;
                default: return 4;
            }
        }
    }
}
