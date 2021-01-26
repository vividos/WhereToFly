using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Model;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for a single weather icon
    /// </summary>
    public class WeatherIconViewModel : ViewModelBase
    {
        /// <summary>
        /// Weather icon description object
        /// </summary>
        public WeatherIconDescription IconDescription { get; }

        #region Binding properties
        /// <summary>
        /// Title of weather icon
        /// </summary>
        public string Title { get => this.IconDescription.Name; }

        /// <summary>
        /// Icon image source for weather icon
        /// </summary>
        public ImageSource Icon { get; private set; }

        /// <summary>
        /// Command that is carried out when weather icon has been tapped.
        /// </summary>
        public ICommand Tapped { get; internal set; }
        #endregion

        /// <summary>
        /// Creates a new weather icon view model from weather icon description
        /// </summary>
        /// <param name="iconDescription">weather icon description to use</param>
        public WeatherIconViewModel(WeatherIconDescription iconDescription)
        {
            this.IconDescription = iconDescription;

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            this.Tapped = new AsyncCommand(this.OpenWeatherIconTargetAsync);

            Task.Run(async () =>
            {
                this.Icon = await WeatherImageCache.GetImageAsync(this.IconDescription);

                this.OnPropertyChanged(nameof(this.Icon));
            });
        }

        /// <summary>
        /// Called in order to open weather icon target
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OpenWeatherIconTargetAsync()
        {
            switch (this.IconDescription.Type)
            {
                case WeatherIconDescription.IconType.IconLink:
                    await NavigationService.Instance.NavigateAsync(
                        Constants.PageKeyWeatherDetailsPage,
                        animated: true,
                        parameter: this.IconDescription);
                    break;

                case WeatherIconDescription.IconType.IconApp:
                    var appManager = DependencyService.Get<IAppManager>();
                    appManager.OpenApp(this.IconDescription.WebLink);
                    break;

                case WeatherIconDescription.IconType.IconPlaceholder:
                    await this.OpenSelectionPageAsync();
                    break;

                default:
                    Debug.Assert(false, "invalid weather icon type");
                    break;
            }
        }

        /// <summary>
        /// Opens weather icon selection page
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OpenSelectionPageAsync()
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
            WeatherDashboardViewModel.AddWeatherIcon(weatherIcon);
            App.RunOnUiThread(async () => await NavigationService.Instance.GoBack());
        }
    }
}
