using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Styles
{
    /// <summary>
    /// Helper for app themes
    /// </summary>
    public static class ThemeHelper
    {
        /// <summary>
        /// Theme that was last set using ChangeTheme
        /// </summary>
        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

        /// <summary>
        /// Changes app theme
        /// </summary>
        /// <param name="theme">new theme to use</param>
        /// <param name="forceUpdate">true to force update, false when not</param>
        public static void ChangeTheme(AppTheme theme, bool forceUpdate)
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

            if (theme == AppTheme.Unspecified)
            {
                // we need to set this to Unspecified first, or RequestedTheme would just contain
                // the last set value, instead of the device's actual theme
                Application.Current.UserAppTheme = OSAppTheme.Unspecified;

                // we also need to reset the platform's theme, or else RequestedTheme would still
                // report the last set platform theme
                platform?.SetPlatformTheme(OSAppTheme.Unspecified);

                var appTheme = Application.Current.RequestedTheme;
                theme = appTheme == OSAppTheme.Dark ? AppTheme.Dark : AppTheme.Light;
            }

            ResourceDictionary newTheme =
                theme == AppTheme.Dark ? new global::WhereToFly.App.Styles.DarkTheme() : new global::WhereToFly.App.Styles.LightTheme();

            var resources = Application.Current.Resources;
            foreach (string item in newTheme.Keys)
            {
                resources[item] = newTheme[item];
            }

            Application.Current.UserAppTheme = OSAppThemeFromAppTheme(theme);

            // apply platform specific changes
            var platformAppTheme = theme == AppTheme.Dark ? OSAppTheme.Dark : OSAppTheme.Light;
            platform?.SetPlatformTheme(platformAppTheme);

            // remember new theme
            CurrentTheme = theme;
        }

        /// <summary>
        /// Translates AppTheme value to OSAppTheme value
        /// </summary>
        /// <param name="theme">app theme value</param>
        /// <returns>OS app theme value</returns>
        private static OSAppTheme OSAppThemeFromAppTheme(AppTheme theme)
        {
            return theme switch
            {
                AppTheme.Light => OSAppTheme.Light,
                AppTheme.Dark => OSAppTheme.Dark,
                _ => OSAppTheme.Unspecified,
            };
        }
    }
}
