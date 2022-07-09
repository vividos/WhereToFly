using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Models;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the weather icon selection popup page.
    /// </summary>
    public partial class SelectWeatherIconViewModel : ViewModelBase
    {
        #region Binding properties
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
        /// <param name="setResult">action to set result</param>
        /// <param name="group">group to filter by, or null to show all groups</param>
        public SelectWeatherIconViewModel(
            Action<WeatherIconDescription> setResult,
            string group = null)
        {
            Debug.Assert(setResult != null, "action must not be null");

            this.SetupBindings(setResult, group);
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        /// <param name="setResult">action to set result</param>
        /// <param name="group">group to filter by, or null to show all groups</param>
        private void SetupBindings(Action<WeatherIconDescription> setResult, string group)
        {
            this.ItemTappedCommand = new Command<WeatherIconListEntryViewModel>(
                (itemViewModel) =>
                {
                    setResult(itemViewModel.IconDescription);

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

            var ungroupedWeatherIconList = new ObservableCollection<WeatherIconListEntryViewModel>(
                from weatherIcon in weatherIconList
                where IsInGroup(@group, weatherIcon) && IsAppAvailable(weatherIcon)
                select new WeatherIconListEntryViewModel(this, weatherIcon));

            var groupedWeatherIconList =
                from weatherIconViewModel in ungroupedWeatherIconList
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
                    (appManager != null && appManager.IsAvailable(weatherIcon.WebLink));
            }
        }

        /// <summary>
        /// Returns a group key for the group text; this is used to order weather icons by group.
        /// </summary>
        /// <param name="group">group text</param>
        /// <returns>group key; an order integer</returns>
        private static int GroupKeyFromGroup(string group)
        {
            return group switch
            {
                "Weather forecast" => 0,
                "Current weather" => 1,
                "Webcams" => 2,
                "Android app" => 3,
                _ => 4,
            };
        }
    }
}
