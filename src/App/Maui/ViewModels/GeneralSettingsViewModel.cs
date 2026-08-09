using WhereToFly.App.Abstractions;
using WhereToFly.App.Models;

namespace WhereToFly.App.ViewModels;

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
    public AppThemeViewModel? SelectedAppTheme
    {
        get
        {
            return this.AppThemeItems.Find(
                x => x.Value == this.appSettings.AppTheme);
        }

        set
        {
            if (value?.Value != null &&
                this.appSettings.AppTheme != value.Value)
            {
                this.appSettings.AppTheme = value.Value;
                App.Settings!.AppTheme = value.Value;
                MainThread.BeginInvokeOnMainThread(
                    async () => await this.SaveThemeSettingsAsync());
            }
        }
    }
    #endregion

    /// <summary>
    /// Creates a new view model for the general settings page
    /// </summary>
    public GeneralSettingsViewModel()
    {
        this.appSettings = App.Settings!;

        this.AppThemeItems =
        [
            new AppThemeViewModel("Same as device", AppTheme.Unspecified),
            new AppThemeViewModel("Light theme", AppTheme.Light),
            new AppThemeViewModel("Dark theme", AppTheme.Dark),
        ];
    }

    /// <summary>
    /// Saves settings to data service
    /// </summary>
    /// <returns>task to wait on</returns>
    private async Task SaveThemeSettingsAsync()
    {
        UserInterface.UserAppTheme = this.appSettings.AppTheme;

        var dataService = Services.GetRequiredService<IDataService>();
        await dataService.StoreAppSettingsAsync(this.appSettings);
    }
}
