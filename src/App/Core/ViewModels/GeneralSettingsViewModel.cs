using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the map settings page
    /// </summary>
    public class GeneralSettingsViewModel : ViewModelBase
    {
        #region Binding properties
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
    }
}
