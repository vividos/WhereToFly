using Xamarin.UITest;

namespace WhereToFly.App.UITest
{
    /// <summary>
    /// Map page; using the UI test Page Object Pattern
    /// </summary>
    internal class MapPage : PageBase
    {
        /// <summary>
        /// Creates a new map page from IApp instance that just has started up
        /// </summary>
        /// <param name="app">app under test</param>
        public MapPage(IApp app)
            : base(app)
        {
            this.WaitForCompletePage();
        }

        /// <summary>
        /// Waits for page to complete loading
        /// </summary>
        private void WaitForCompletePage()
        {
            this.app.WaitForElement(x => x.Marked("ExploreMapWebView"));
            this.app.WaitForElement(x => x.WebView());
        }
    }
}
