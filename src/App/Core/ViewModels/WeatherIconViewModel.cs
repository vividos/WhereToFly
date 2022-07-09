using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Services;
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
        /// Function to call to start adding a new weather icon
        /// </summary>
        private readonly Func<Task> funcAddWeatherIcon;

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
        /// <param name="funcAddWeatherIcon">function to start adding a new weather icon</param>
        /// <param name="iconDescription">weather icon description to use</param>
        public WeatherIconViewModel(
            Func<Task> funcAddWeatherIcon,
            WeatherIconDescription iconDescription)
        {
            this.funcAddWeatherIcon = funcAddWeatherIcon;
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
                        PageKey.WeatherDetailsPage,
                        animated: true,
                        parameter: this.IconDescription);
                    break;

                case WeatherIconDescription.IconType.IconApp:
                    var appManager = DependencyService.Get<IAppManager>();
                    appManager.OpenApp(this.IconDescription.WebLink);
                    break;

                case WeatherIconDescription.IconType.IconPlaceholder:
                    await this.funcAddWeatherIcon?.Invoke();
                    break;

                default:
                    Debug.Assert(false, "invalid weather icon type");
                    break;
            }
        }
    }
}
