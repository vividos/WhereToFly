namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Shows an info page displaying a CarouselView with the info (sub) pages.
    /// </summary>
    public partial class InfoPage : ContentPage
    {
        /// <summary>
        /// Creates a new info page
        /// </summary>
        public InfoPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Called when web view navigates to a new URL
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnNavigating_WebView(object sender, WebNavigatingEventArgs args)
        {
            if (args.NavigationEvent == WebNavigationEvent.NewPage &&
                args.Url.StartsWith("http"))
            {
                args.Cancel = true;

                MainThread.BeginInvokeOnMainThread(
                    () => Browser.OpenAsync(args.Url, BrowserLaunchMode.External));
            }
        }
    }
}
