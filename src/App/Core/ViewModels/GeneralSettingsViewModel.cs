using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Styles;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the map settings page
    /// </summary>
    public class GeneralSettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// View model for app theme
        /// </summary>
        public class AppThemeViewModel
        {
            /// <summary>
            /// Display text for value
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// App theme value
            /// </summary>
            public Theme Value { get; set; }
        }

        /// <summary>
        /// App settings object
        /// </summary>
        private readonly AppSettings appSettings;

        #region Binding properties
        /// <summary>
        /// List of available map overlay types
        /// </summary>
        public List<AppThemeViewModel> AppThemeItems
        {
            get; private set;
        }

        /// <summary>
        /// Currently selected app theme
        /// </summary>
        public AppThemeViewModel SelectedAppTheme
        {
            get
            {
                return this.AppThemeItems.Find(x => x.Value == this.appSettings.AppTheme);
            }

            set
            {
                if (this.appSettings.AppTheme != value.Value)
                {
                    this.appSettings.AppTheme = value.Value;
                    App.Settings.AppTheme = value.Value;
                    Task.Run(async () => await this.SaveThemeSettingsAsync());
                }
            }
        }

        /// <summary>
        /// Username for the alptherm web page
        /// </summary>
        public string AlpthermUsername { get; set; }

        /// <summary>
        /// Password for the alptherm web page
        /// </summary>
        public string AlpthermPassword { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model for the general settings page
        /// </summary>
        public GeneralSettingsViewModel()
        {
            this.appSettings = App.Settings;

            this.AppThemeItems = new List<AppThemeViewModel>
            {
                new AppThemeViewModel { Text = "Same as device", Value = Theme.Device },
                new AppThemeViewModel { Text = "Light theme", Value = Theme.Light },
                new AppThemeViewModel { Text = "Dark theme", Value = Theme.Dark },
            };

            Task.Run(this.LoadDataAsync);
        }

        /// <summary>
        /// Loads view model data from secure storage
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadDataAsync()
        {
            this.AlpthermUsername = await SecureStorage.GetAsync(Constants.SecureSettingsAlpthermUsername);
            this.OnPropertyChanged(nameof(this.AlpthermUsername));

            this.AlpthermPassword = await SecureStorage.GetAsync(Constants.SecureSettingsAlpthermPassword);
            this.OnPropertyChanged(nameof(this.AlpthermPassword));
        }

        /// <summary>
        /// Stores view model data to secure storage
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task StoreDataAsync()
        {
            if (this.AlpthermUsername != null)
            {
                await SecureStorage.SetAsync(Constants.SecureSettingsAlpthermUsername, this.AlpthermUsername);
            }

            if (this.AlpthermPassword != null)
            {
                await SecureStorage.SetAsync(Constants.SecureSettingsAlpthermPassword, this.AlpthermPassword);
            }
        }

        /// <summary>
        /// Saves settings to data service
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task SaveThemeSettingsAsync()
        {
            ThemeHelper.ChangeTheme(this.appSettings.AppTheme, true);

            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(this.appSettings);
        }
    }
}
