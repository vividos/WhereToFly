using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Tabbed settings page to show all settings pages
    /// </summary>
    public class SettingsPage : TabbedPage
    {
        /// <summary>
        /// Creates new settings page
        /// </summary>
        public SettingsPage()
        {
            this.Title = "Settings";

            this.Children.Add(new GeneralSettingsPage());
            this.Children.Add(new MapSettingsPage());
        }
    }
}
