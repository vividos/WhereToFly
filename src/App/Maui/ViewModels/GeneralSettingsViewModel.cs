using WhereToFly.App.Models;

namespace WhereToFly.App.ViewModels
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
            public AppTheme Value { get; set; }

            /// <summary>
            /// Creates a new app theme view model
            /// </summary>
            /// <param name="text">display text</param>
            /// <param name="appTheme">app theme</param>
            public AppThemeViewModel(string text, AppTheme appTheme)
            {
                this.Text = text;
                this.Value = appTheme;
            }
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
                    App.Settings!.AppTheme = value.Value;
                    Task.Run(async () => await this.SaveThemeSettingsAsync());
                }
            }
        }

        /// <summary>
        /// Username for the alptherm web page
        /// </summary>
        public string AlpthermUsername { get; set; } = string.Empty;

        /// <summary>
        /// Password for the alptherm web page
        /// </summary>
        public string AlpthermPassword { get; set; } = string.Empty;
        #endregion

        /// <summary>
        /// Creates a new view model for the general settings page
        /// </summary>
        public GeneralSettingsViewModel()
        {
            this.appSettings = App.Settings!;

            this.AppThemeItems = new List<AppThemeViewModel>
            {
                new AppThemeViewModel("Same as device", AppTheme.Unspecified),
                new AppThemeViewModel("Light theme", AppTheme.Light),
                new AppThemeViewModel("Dark theme", AppTheme.Dark),
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
            var userInterface = DependencyService.Get<IUserInterface>();
            userInterface.UserAppTheme = this.appSettings.AppTheme;

            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(this.appSettings);
        }
    }
}
