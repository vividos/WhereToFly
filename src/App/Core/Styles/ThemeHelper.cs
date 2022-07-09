using WhereToFly.App.Core.Models;
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

            var platform = DependencyService.Get<IPlatform>();

            if (theme == Theme.Device)
            {
                // we need to set this to Unspecified first, or RequestedTheme would just contain
                // the last set value, instead of the device's actual theme
                Application.Current.UserAppTheme = OSAppTheme.Unspecified;

                // we also need to reset the platform's theme, or else RequestedTheme would still
                // report the last set platform theme
                platform?.SetPlatformTheme(OSAppTheme.Unspecified);

                OSAppTheme appTheme = Application.Current.RequestedTheme;
                theme = appTheme == OSAppTheme.Dark ? Theme.Dark : Theme.Light;
            }

            ResourceDictionary newTheme =
                theme == Theme.Dark ? new DarkTheme() : new LightTheme();

            var resources = Application.Current.Resources;
            foreach (var item in newTheme.Keys)
            {
                resources[item] = newTheme[item];
            }

            Application.Current.UserAppTheme = OSAppThemeFromTheme(theme);

            // apply platform specific changes
            var platformAppTheme = theme == Theme.Dark ? OSAppTheme.Dark : OSAppTheme.Light;
            platform?.SetPlatformTheme(platformAppTheme);

            // remember new theme
            CurrentTheme = theme;
        }

        /// <summary>
        /// Translates Theme value to OSAppTheme value
        /// </summary>
        /// <param name="theme">theme value</param>
        /// <returns>OS app theme value</returns>
        private static OSAppTheme OSAppThemeFromTheme(Theme theme)
        {
            return theme switch
            {
                Theme.Light => OSAppTheme.Light,
                Theme.Dark => OSAppTheme.Dark,
                _ => OSAppTheme.Unspecified,
            };
        }
    }
}
