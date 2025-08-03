using WhereToFly.App.Abstractions;
using WhereToFly.App.Logic;
using WhereToFly.App.Resources;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the info page
    /// </summary>
    public class InfoPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Base path to use in WebView control
        /// </summary>
#pragma warning disable S1075 // URIs should not be hardcoded
#if ANDROID
        private static readonly string WebViewBasePath = "file:///android_asset/";
#elif WINDOWS
        private static readonly string WebViewBasePath = "https://appdir/";
#else
        private static readonly string WebViewBasePath = "about:blank";
#endif
#pragma warning restore S1075 // URIs should not be hardcoded

        /// <summary>
        /// View model for one info page entry
        /// </summary>
        public class InfoPageEntryViewModel
        {
            /// <summary>
            /// Header image of info page entry
            /// </summary>
            public ImageSource? Image { get; set; }

            /// <summary>
            /// Web view source for info page entry
            /// </summary>
            public WebViewSource? WebViewSource { get; set; }
        }

        /// <summary>
        /// All info pages
        /// </summary>
        public List<InfoPageEntryViewModel>? Pages { get; set; }

        /// <summary>
        /// Creates a new info page view model
        /// </summary>
        public InfoPageViewModel()
        {
            Task.Run(this.InitViewModel);
        }

        /// <summary>
        /// Initializes view model
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitViewModel()
        {
#if WINDOWS
            string pathPrefix = "Resources/Assets/";
#else
            string pathPrefix = string.Empty;
#endif

            this.Pages =
            [
                new InfoPageEntryViewModel
                {
                    Image = ImageSource.FromStream(
                        async (cancellationToken) => await Assets.Get("info/version.jpg")),
                    WebViewSource = await GetWebViewSource("info/version.md"),
                },
                new InfoPageEntryViewModel
                {
                    Image = null,
                    WebViewSource = await GetWebViewSource("info/manual.md"),
                },
                new InfoPageEntryViewModel
                {
                    Image = null,
                    WebViewSource = await GetWebViewSource(pathPrefix + "info/Changelog.md"),
                },
                new InfoPageEntryViewModel
                {
                    Image = null,
                    WebViewSource = await GetWebViewSource(pathPrefix + "info/Credits.md"),
                },
            ];

            this.OnPropertyChanged(nameof(this.Pages));
        }

        /// <summary>
        /// Returns web view source to display content of given markdown file
        /// </summary>
        /// <param name="markdownFilename">markdown filename, relative to the asset folder</param>
        /// <returns>web view source</returns>
        private static async Task<WebViewSource> GetWebViewSource(
            string markdownFilename)
        {
            string htmlText = await GetHtmlText(markdownFilename);

            return new HtmlWebViewSource
            {
                Html = htmlText,
                BaseUrl = WebViewBasePath + "info/",
            };
        }

        /// <summary>
        /// Returns HTML text to display in web view, from given markdown file
        /// </summary>
        /// <param name="markdownFilename">markdown filename, relative to the asset folder</param>
        /// <returns>HTML text</returns>
        private static async Task<string> GetHtmlText(string markdownFilename)
        {
            using var stream = await Assets.Get(markdownFilename);
            if (stream == null)
            {
                return string.Empty;
            }

            using var reader = new StreamReader(stream);
            string markdownText = await reader.ReadToEndAsync();

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

            var userInterface = DependencyService.Get<IUserInterface>();
            if (!userInterface.IsDarkTheme)
            {
                htmlText = "<style> body { color: black; background-color: white; } .svg-img { filter: invert(100%); } </style>" + htmlText;
            }
            else
            {
                htmlText = HtmlConverter.AddTextColorStyles(
                    htmlText,
                    "white",
                    "black",
                    App.GetResourceColor("AccentColor", true));
            }

            return htmlText;
        }

        /// <summary>
        /// Returns the app's current version number as displayable text
        /// </summary>
        /// <returns>version number as text</returns>
        private static string GetCurrentVersionText()
            => ThisAssembly.AssemblyInformationalVersion;
    }
}
