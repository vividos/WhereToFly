using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Views;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the weather dashboard page
    /// </summary>
    public class WeatherDashboardViewModel : ViewModelBase
    {
        /// <summary>
        /// List of weather icon descriptions to display
        /// </summary>
        private List<WeatherIconDescription> weatherIconDescriptionList;

        #region Bindings properties
        /// <summary>
        /// List of weather icon view models for all weather icons to display
        /// </summary>
        public ObservableCollection<WeatherIconViewModel> WeatherDashboardItems { get; set; } =
            new ObservableCollection<WeatherIconViewModel>();

        /// <summary>
        /// Command to execute when "add bookmark" menu item is selected
        /// </summary>
        public ICommand AddWebLinkCommand { get; set; }

        /// <summary>
        /// Command to execute when "add icon" menu item is selected
        /// </summary>
        public ICommand AddIconCommand { get; set; }

        /// <summary>
        /// Command to execute when "clear all" menu item is selected
        /// </summary>
        public ICommand ClearAllCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model for the weather dashboard
        /// </summary>
        public WeatherDashboardViewModel()
        {
            this.SetupBindings();
        }

        /// <summary>
        /// Called when a weather icon should be added to the dashboard
        /// </summary>
        /// <param name="iconDescription">weather icon description</param>
        /// <returns>task to wait on</returns>
        public async Task AddWeatherIcon(WeatherIconDescription iconDescription)
        {
            // remove it when it's already in the list, in order to move it to the end
            this.weatherIconDescriptionList.RemoveAll(
                x => x.Type == iconDescription.Type && x.WebLink == iconDescription.WebLink);

            this.weatherIconDescriptionList.Add(iconDescription);

            await this.SaveWeatherIconListAsync();

            this.UpdateWeatherDashboardItems();
        }

        /// <summary>
        /// Saves current weather icon description list, to storage
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task SaveWeatherIconListAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            var weatherDashboardIconDataService = dataService.GetWeatherDashboardIconDataService();

            await weatherDashboardIconDataService.ClearList();
            await weatherDashboardIconDataService.AddList(
                this.weatherIconDescriptionList);
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            this.AddIconCommand = new AsyncCommand(this.AddIconAsync);
            this.AddWebLinkCommand = new AsyncCommand(this.AddWebLinkAsync);
            this.ClearAllCommand = new AsyncCommand(this.ClearAllWeatherIcons);

            Task.Run(async () => await this.InitWeatherIconDescriptionList());
        }

        /// <summary>
        /// Initializes weather icon description list
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitWeatherIconDescriptionList()
        {
            var dataService = DependencyService.Get<IDataService>();
            var weatherDashboardIconDataService = dataService.GetWeatherDashboardIconDataService();

            this.weatherIconDescriptionList =
                (await weatherDashboardIconDataService.GetList()).ToList();

            this.UpdateWeatherDashboardItems();
        }

        /// <summary>
        /// Adds a weather icon to the current dashboard
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task AddIconAsync()
        {
            var weatherIcon =
                await NavigationService.Instance.NavigateToPopupPageAsync<WeatherIconDescription>(
                    PopupPageKey.SelectWeatherIconPopupPage,
                    animated: true);

            if (weatherIcon != null)
            {
                await this.AddWeatherIcon(weatherIcon);
            }
        }

        /// <summary>
        /// Called when the user clicked on the "add web link" button
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task AddWebLinkAsync()
        {
            WeatherIconDescription weatherIconDescription =
                await AddWeatherLinkPopupPage.ShowAsync();

            if (weatherIconDescription == null)
            {
                return;
            }

            var dataService = DependencyService.Get<IDataService>();
            var weatherIconDescriptionDataService = dataService.GetWeatherIconDescriptionDataService();
            await weatherIconDescriptionDataService.Add(weatherIconDescription);

            int insertIndex = this.WeatherDashboardItems.Any()
                ? this.WeatherDashboardItems.Count - 1
                : 0;

            this.WeatherDashboardItems.Insert(
                insertIndex,
                new WeatherIconViewModel(this.AddIconAsync, weatherIconDescription));

            this.OnPropertyChanged(nameof(this.WeatherDashboardItems));
        }

        /// <summary>
        /// Clears all weather icons
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ClearAllWeatherIcons()
        {
            this.weatherIconDescriptionList.Clear();
            await this.SaveWeatherIconListAsync();

            this.UpdateWeatherDashboardItems();
        }

        /// <summary>
        /// Updates the weather icon view model list from the icon list.
        /// </summary>
        private void UpdateWeatherDashboardItems()
        {
            var weatherIconViewModels =
                (from iconDescription in this.weatherIconDescriptionList
                 select new WeatherIconViewModel(this.AddIconAsync, iconDescription)).ToList();

            weatherIconViewModels.Add(
                new WeatherIconViewModel(
                    this.AddIconAsync,
                    new WeatherIconDescription
                    {
                        Name = "Add new...",
                        Type = WeatherIconDescription.IconType.IconPlaceholder,
                    }));

            this.WeatherDashboardItems = new ObservableCollection<WeatherIconViewModel>(weatherIconViewModels);

            this.OnPropertyChanged(nameof(this.WeatherDashboardItems));
        }
    }
}
