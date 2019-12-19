using WhereToFly.App.Logic;
using Xamarin.Essentials;
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

            string markdownText = platform.LoadAssetText("info/version.md");

            string versionText;
            try
            {
                versionText = $"{AppInfo.VersionString} (Build {AppInfo.BuildString})";
            }
            catch (System.Exception)
            {
                versionText = "Unknown version";
            }

            markdownText = markdownText.Replace("%VERSION%", versionText);

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
