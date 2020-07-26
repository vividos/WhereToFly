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
        public static Theme CurrentTheme { get; private set; } = Theme.Light;

        /// <summary>
        /// Changes app theme
        /// </summary>
        /// <param name="theme">new theme to use</param>
        /// <param name="forceUpdate">true to force update, false when not</param>
        public static void ChangeTheme(Theme theme, bool forceUpdate)
        {
            if (!forceUpdate && CurrentTheme == theme)
            {
                return;
            }

            // switch to UI thread; or else accessing RequestedTheme on UWP crashes
            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() => ChangeTheme(theme, forceUpdate));
                return;
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

            CurrentTheme = theme;
        }
    }
}
