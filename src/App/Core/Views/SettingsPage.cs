using System.Threading.Tasks;
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

            var currentPageIndex = App.Settings.LastShownSettingsPage;

            if (currentPageIndex < 0 || currentPageIndex >= this.Children.Count)
            {
                currentPageIndex = 0;
            }

            this.CurrentPage = this.Children[currentPageIndex];

            this.CurrentPageChanged += async (sender, args) => await this.OnCurrentSettingsPageChanged();
        }

        /// <summary>
        /// Called when the currently shwon page has changed
        /// </summary>
        /// <returns>task to wait on</returns>
        protected async Task OnCurrentSettingsPageChanged()
        {
            int currentPageIndex = this.Children.IndexOf(this.CurrentPage);
            if (currentPageIndex == -1)
            {
                return;
            }

            App.Settings.LastShownSettingsPage = currentPageIndex;

            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(App.Settings);
        }
    }
}
