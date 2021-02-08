using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
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

            Task.Run(async () =>
            {
                var dataService = DependencyService.Get<IDataService>();

                var weatherIconDescriptionDataService = dataService.GetWeatherIconDescriptionDataService();
                var weatherIconList = await weatherIconDescriptionDataService.GetList();

                this.WeatherIconList = new ObservableCollection<WeatherIconListEntryViewModel>(
                    from weatherIcon in weatherIconList
                    where @group == null || @group == weatherIcon.Group
                    select new WeatherIconListEntryViewModel(weatherIcon));

                this.OnPropertyChanged(nameof(this.WeatherIconList));
            });
        }
    }
}
