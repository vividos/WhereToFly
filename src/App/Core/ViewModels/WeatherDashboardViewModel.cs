using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core.Services;
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
        /// Command to execute when "add new" menu item is selected
        /// </summary>
        public Command AddNewCommand { get; set; }

        /// <summary>
        /// Command to execute when "clear all" menu item is selected
        /// </summary>
        public Command ClearAllCommand { get; set; }
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
            MessagingCenter.Subscribe<object, WeatherIconDescription>(this, MessageAddWeatherIcon, this.OnAddWeatherIcon);

            this.SetupBindings();
        }

        /// <summary>
        /// Adds new weather icon to dashboard; called by SelectWeatherIconViewModel
        /// </summary>
        /// <param name="sender">sender object; must not be null</param>
        /// <param name="iconDescription">weather icon description</param>
        public static void AddWeatherIcon(object sender, WeatherIconDescription iconDescription)
        {
            MessagingCenter.Send<object, WeatherIconDescription>(sender, MessageAddWeatherIcon, iconDescription);
        }

        /// <summary>
        /// Called when a weather icon should be added to the dashboard
        /// </summary>
        /// <param name="sender">sender; always null</param>
        /// <param name="iconDescription">weather icon description</param>
        private void OnAddWeatherIcon(object sender, WeatherIconDescription iconDescription)
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

            var weatherIconDescriptionDataService = dataService.GetWeatherIconDescriptionDataService();

            await weatherIconDescriptionDataService.ClearList();
            await weatherIconDescriptionDataService.AddList(weatherIconDescriptionList);
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            this.WeatherIconDescriptionList = new List<WeatherIconDescription>();

            this.AddNewCommand = new Command(async () =>
            {
                Action<WeatherIconDescription> parameter = this.OnSelectedWeatherIcon;

                await NavigationService.Instance.NavigateAsync(
                    Constants.PageKeySelectWeatherIconPage,
                    animated: true,
                    parameter: parameter);
            });

            this.ClearAllCommand = new Command(async () => await this.ClearAllWeatherIcons());

            Task.Run(async () =>
            {
                var dataService = DependencyService.Get<IDataService>();
                var weatherIconDescriptionDataService = dataService.GetWeatherIconDescriptionDataService();

                this.WeatherIconDescriptionList = (await weatherIconDescriptionDataService.GetList()).ToList();

                this.WeatherIconDescriptionList.Add(
                    new WeatherIconDescription
                    {
                        Name = "Add new...",
                        Type = WeatherIconDescription.IconType.IconPlaceholder,
                    });

                this.OnPropertyChanged(nameof(this.WeatherIconDescriptionList));
            });
        }

        /// <summary>
        /// Called from SelectWeatherIconPage when a weather icon was selected by the user.
        /// </summary>
        /// <param name="weatherIcon">selected weather icon</param>
        private void OnSelectedWeatherIcon(WeatherIconDescription weatherIcon)
        {
            AddWeatherIcon(this, weatherIcon);
            App.RunOnUiThread(async () => await NavigationService.Instance.GoBack());
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
    }
}
