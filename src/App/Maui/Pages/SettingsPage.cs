using WhereToFly.App.Abstractions;

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Tabbed settings page to show all settings pages
    /// </summary>
    public class SettingsPage : TabbedPage
    {
        /// <summary>
        /// Data service
        /// </summary>
        private readonly IDataService dataService;

        /// <summary>
        /// Creates new settings page
        /// </summary>
        /// <param name="services">service provider</param>
        public SettingsPage(IServiceProvider services)
        {
            this.Title = "Settings";

            this.dataService = services.GetRequiredService<IDataService>();

            this.Children.Add(new GeneralSettingsPage());
            this.Children.Add(new MapSettingsPage());

            int currentPageIndex = App.Settings!.LastShownSettingsPage;

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

            await App.InitializedTask;

            App.Settings!.LastShownSettingsPage = currentPageIndex;

            await this.dataService.StoreAppSettingsAsync(App.Settings);
        }
    }
}
