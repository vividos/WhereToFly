using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Model;
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
        /// Current location list; may be filtered by filter text
        /// </summary>
        public ObservableCollection<WeatherIconListEntryViewModel> WeatherIconList { get; set; }

        /// <summary>
        /// Command to execute when an item in the weather icon list has been tapped
        /// </summary>
        public Command<WeatherIconDescription> ItemTappedCommand { get; private set; }
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
            this.ItemTappedCommand = new Command<WeatherIconDescription>(
                (weatherIcon) => tcs.SetResult(weatherIcon));

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
                select new WeatherIconListEntryViewModel(weatherIcon));

            this.OnPropertyChanged(nameof(this.WeatherIconList));

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
    }
}
