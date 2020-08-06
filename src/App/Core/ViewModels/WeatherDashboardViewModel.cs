using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Views;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the weather dashboard page
    /// </summary>
    internal class WeatherDashboardViewModel : ViewModelBase
    {
        #region Bindings properties
        /// <summary>
        /// List of weather icon descriptions for all weather icons to display
        /// </summary>
        public List<WeatherIconDescription> WeatherIconDescriptionList { get; set; }

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
        /// MessagingCenter message constant to show toast message
        /// </summary>
        private const string MessageAddWeatherIcon = "AddWeatherIcon";

        /// <summary>
        /// Creates a new view model for the weather dashboard
        /// </summary>
        public WeatherDashboardViewModel()
        {
            MessagingCenter.Subscribe<WeatherIconDescription>(this, MessageAddWeatherIcon, this.OnAddWeatherIcon);

            this.SetupBindings();
        }

        /// <summary>
        /// Adds new weather icon to dashboard; called by SelectWeatherIconViewModel
        /// </summary>
        /// <param name="iconDescription">weather icon description</param>
        public static void AddWeatherIcon(WeatherIconDescription iconDescription)
        {
            MessagingCenter.Send(iconDescription, MessageAddWeatherIcon);
        }

        /// <summary>
        /// Called when a weather icon should be added to the dashboard
        /// </summary>
        /// <param name="iconDescription">weather icon description</param>
        private void OnAddWeatherIcon(WeatherIconDescription iconDescription)
        {
            // remove it when it's already in the list, in order to move it to the end
            this.WeatherIconDescriptionList.RemoveAll(
                x => x.Type == iconDescription.Type && x.WebLink == iconDescription.WebLink);

            int insertIndex = this.WeatherIconDescriptionList.Any() ? this.WeatherIconDescriptionList.Count - 1 : 0;
            this.WeatherIconDescriptionList.Insert(insertIndex, iconDescription);

            this.OnPropertyChanged(nameof(this.WeatherIconDescriptionList));

            Task.Run(async () =>
            {
                await this.SaveWeatherIconListAsync();
            });
        }

        /// <summary>
        /// Saves weather icon list, without placeholder icon, to storage
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task SaveWeatherIconListAsync()
        {
            var dataService = DependencyService.Get<IDataService>();

            var weatherIconDescriptionList = new List<WeatherIconDescription>(this.WeatherIconDescriptionList);
            weatherIconDescriptionList.RemoveAll(x => x.Type == WeatherIconDescription.IconType.IconPlaceholder);

            var weatherDashboardIconDataService = dataService.GetWeatherDashboardIconDataService();

            await weatherDashboardIconDataService.ClearList();
            await weatherDashboardIconDataService.AddList(weatherIconDescriptionList);
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            this.WeatherIconDescriptionList = new List<WeatherIconDescription>();

            this.AddIconCommand = new AsyncCommand(this.AddIconAsync);
            this.AddWebLinkCommand = new AsyncCommand(this.AddWebLinkAsync);
            this.ClearAllCommand = new AsyncCommand(this.ClearAllWeatherIcons);

            Task.Run(async () => await this.UpdateWeatherIconDescriptionList());
        }

        /// <summary>
        /// Adds a weather icon to the current dashboard
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task AddIconAsync()
        {
            Action<WeatherIconDescription> parameter = this.OnSelectedWeatherIcon;

            await NavigationService.Instance.NavigateAsync(
                Constants.PageKeySelectWeatherIconPage,
                animated: true,
                parameter: parameter);
        }

        /// <summary>
        /// Called from SelectWeatherIconPage when a weather icon was selected by the user.
        /// </summary>
        /// <param name="weatherIcon">selected weather icon</param>
        private void OnSelectedWeatherIcon(WeatherIconDescription weatherIcon)
        {
            AddWeatherIcon(weatherIcon);
            App.RunOnUiThread(async () => await NavigationService.Instance.GoBack());
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

            int insertIndex = this.WeatherIconDescriptionList.Any()
                ? this.WeatherIconDescriptionList.Count - 1
                : 0;

            this.WeatherIconDescriptionList.Insert(
                insertIndex,
                weatherIconDescription);

            this.OnPropertyChanged(nameof(this.WeatherIconDescriptionList));

            await this.SaveWeatherIconListAsync();
        }

        /// <summary>
        /// Clears all weather icons
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ClearAllWeatherIcons()
        {
            this.WeatherIconDescriptionList.Clear();

            this.WeatherIconDescriptionList.Add(
                new WeatherIconDescription
                {
                    Name = "Add new...",
                    Type = WeatherIconDescription.IconType.IconPlaceholder,
                });

            this.OnPropertyChanged(nameof(this.WeatherIconDescriptionList));

            await this.SaveWeatherIconListAsync();
        }

        /// <summary>
        /// Updates the weather icon description list from the data service.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task UpdateWeatherIconDescriptionList()
        {
            var dataService = DependencyService.Get<IDataService>();
            var weatherDashboardIconDataService = dataService.GetWeatherDashboardIconDataService();

            this.WeatherIconDescriptionList = (await weatherDashboardIconDataService.GetList()).ToList();

            this.WeatherIconDescriptionList.Add(
                new WeatherIconDescription
                {
                    Name = "Add new...",
                    Type = WeatherIconDescription.IconType.IconPlaceholder,
                });

            this.OnPropertyChanged(nameof(this.WeatherIconDescriptionList));
        }
    }
}
