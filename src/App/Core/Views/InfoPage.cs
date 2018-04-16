using WhereToFly.App.Logic;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Info page, showing markdown from the info.md in the Assets folder.
    /// </summary>
    public class InfoPage : ContentPage
    {
        /// <summary>
        /// Creates new info page object
        /// </summary>
        public InfoPage()
        {
            this.InitLayout();
        }

        /// <summary>
        /// Initializes layout
        /// </summary>
        private void InitLayout()
        {
            this.Title = "Info";

            var platform = DependencyService.Get<IPlatform>();

            string markdownText = platform.LoadAssetText("info.md");

            markdownText = markdownText.Replace("%VERSION%", platform.AppVersionNumber);

            string htmlText = HtmlConverter.FromMarkdown(markdownText);

            var webViewSource = new HtmlWebViewSource
            {
                BaseUrl = platform.WebViewBasePath,
                Html = htmlText
            };

            this.Content = new WebView
            {
                Source = webViewSource
            };
        }
    }
}
