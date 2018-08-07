using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for a single weather icon
    /// </summary>
    public class WeatherIconViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Weather icon description object
        /// </summary>
        private readonly WeatherIconDescription iconDescription;

        #region Binding properties
        /// <summary>
        /// Title of weather icon
        /// </summary>
        public string Title { get => this.iconDescription.Name; }

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
            this.iconDescription = iconDescription;

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            this.Tapped = new Command(async () => await this.OpenWeatherIconTargetAsync());

            Task.Run(async () =>
            {
                var imageCache = DependencyService.Get<WeatherImageCache>();

                this.Icon = await imageCache.GetImageAsync(this.iconDescription);

                this.OnPropertyChanged(nameof(this.Icon));
            });
        }

        /// <summary>
        /// Called in order to open weather icon target
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OpenWeatherIconTargetAsync()
        {
            switch (this.iconDescription.Type)
            {
                case WeatherIconDescription.IconType.IconLink:
                    await NavigationService.Instance.NavigateAsync(
                        Constants.PageKeyWeatherDetailsPage,
                        animated: true,
                        parameter: this.iconDescription);
                    break;

                case WeatherIconDescription.IconType.IconApp:
                    var appManager = DependencyService.Get<IAppManager>();
                    appManager.OpenApp(this.iconDescription.WebLink);
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
            WeatherDashboardViewModel.AddWeatherIcon(this, weatherIcon);
            App.RunOnUiThread(async () => await NavigationService.Instance.GoBack());
        }

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Event that gets signaled when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Call this method to signal that a property has changed
        /// </summary>
        /// <param name="propertyName">property name; use C# 6 nameof() operator</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
