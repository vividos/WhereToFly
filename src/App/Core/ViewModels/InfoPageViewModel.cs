using System;
using System.Collections.Generic;
using WhereToFly.App.Core.Logic;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the info page
    /// </summary>
    public class InfoPageViewModel
    {
        /// <summary>
        /// View model for one info page entry
        /// </summary>
        public class InfoPageEntryViewModel
        {
            /// <summary>
            /// Header image of info page entry
            /// </summary>
            public ImageSource Image { get; set; }

            /// <summary>
            /// Web view source for info page entry
            /// </summary>
            public WebViewSource WebViewSource { get; set; }
        }

        /// <summary>
        /// All info pages
        /// </summary>
        public List<InfoPageEntryViewModel> Pages { get; set; }

        /// <summary>
        /// Creates a new info page view model
        /// </summary>
        public InfoPageViewModel()
        {
            var platform = DependencyService.Get<IPlatform>();

            this.Pages = new List<InfoPageEntryViewModel>
            {
                new InfoPageEntryViewModel
                {
                    Image = ImageSource.FromStream(() => platform.OpenAssetStream("info/version.jpg")),
                    WebViewSource = GetWebViewSource("info/version.md"),
                },
                new InfoPageEntryViewModel
                {
                    Image = null,
                    WebViewSource = GetWebViewSource("info/manual.md"),
                },
                new InfoPageEntryViewModel
                {
                    Image = null,
                    WebViewSource = GetWebViewSource("info/Changelog.md"),
                },
                new InfoPageEntryViewModel
                {
                    Image = null,
                    WebViewSource = GetWebViewSource("info/Credits.md"),
                },
            };
        }

        /// <summary>
        /// Returns web view source to display content of given markdown file
        /// </summary>
        /// <param name="markdownFilename">markdown filename, relative to the asset folder</param>
        /// <returns>web view source</returns>
        private static WebViewSource GetWebViewSource(string markdownFilename)
        {
            string htmlText = GetHtmlText(markdownFilename);

            IPlatform platform = DependencyService.Get<IPlatform>();

            return new HtmlWebViewSource
            {
                Html = htmlText,
                BaseUrl = platform.WebViewBasePath + "info/",
            };
        }

        /// <summary>
        /// Returns HTML text to display in web view, from given markdown file
        /// </summary>
        /// <param name="markdownFilename">markdown filename, relative to the asset folder</param>
        /// <returns>HTML text</returns>
        private static string GetHtmlText(string markdownFilename)
        {
            IPlatform platform = DependencyService.Get<IPlatform>();
            string markdownText = platform.LoadAssetText(markdownFilename);

            if (markdownFilename.EndsWith("version.md"))
            {
                markdownText = markdownText.Replace("%VERSION%", GetCurrentVersionText());
            }

            string htmlText = HtmlConverter.FromMarkdown(markdownText);

            if (markdownFilename.EndsWith("manual.md"))
            {
                htmlText = "<style> img { max-width: 90vw; }</style>" + htmlText;
            }

            htmlText = htmlText.Replace(".svg\"", ".svg\" class=\"svg-img\"");

            if (Styles.ThemeHelper.CurrentTheme == Models.Theme.Light)
            {
                htmlText = "<style> body { color: black; background-color: white; } .svg-img { filter: invert(100%); } </style>" + htmlText;
            }
            else
            {
                htmlText = HtmlConverter.AddTextColorStyles(
                    htmlText,
                    "white",
                    "black",
                    App.GetResourceColor("AccentColor"));
            }

            return htmlText;
        }

        /// <summary>
        /// Returns the app's current version number as displayable text
        /// </summary>
        /// <returns>version number as text</returns>
        private static string GetCurrentVersionText()
        {
            try
            {
                return $"{AppInfo.VersionString} (Build {AppInfo.BuildString})";
            }
            catch (Exception)
            {
                return "Unknown version";
            }
        }
    }
}
