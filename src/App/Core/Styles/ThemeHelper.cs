using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Model;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Styles
{
    /// <summary>
    /// Helper for app themes
    /// </summary>
    public static class ThemeHelper
    {
        /// <summary>
        /// Theme that was last set using ChangeTheme
        /// </summary>
        public static Theme LastTheme { get; private set; }

        /// <summary>
        /// Changes app theme
        /// </summary>
        /// <param name="theme">new theme to use</param>
        /// <param name="forceUpdate">true to force update, false when not</param>
        /// <returns>task to wait on</returns>
        public static async Task ChangeTheme(Theme theme, bool forceUpdate)
        {
            // switch to UI thread; or else on UWP accessing RequestedTheme crashes
            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(() => ChangeTheme(theme, forceUpdate));
                return;
            }

            if (!forceUpdate)
            {
                var dataService = DependencyService.Get<IDataService>();
                var appSettings = await dataService.GetAppSettingsAsync(CancellationToken.None);
                if (appSettings.AppTheme == theme)
                {
                    return;
                }
            }

            if (theme == Theme.Device)
            {
                AppTheme appTheme = AppInfo.RequestedTheme;
                theme = appTheme == AppTheme.Dark ? Theme.Dark : Theme.Light;
            }

            ResourceDictionary newTheme =
                theme == Theme.Dark ? new DarkTheme() : (ResourceDictionary)new LightTheme();

            var resources = Application.Current.Resources;
            foreach (var item in newTheme.Keys)
            {
                resources[item] = newTheme[item];
            }

            LastTheme = theme;
        }
    }
}
