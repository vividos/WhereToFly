using Windows.UI.ViewManagement;

namespace WhereToFly.App.UWP
{
    /// <summary>
    /// Main page of the UWP app
    /// </summary>
    public sealed partial class MainPage
    {
        /// <summary>
        /// Creates new main page
        /// </summary>
        public MainPage()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = titleBar.ButtonBackgroundColor =
                Windows.UI.Color.FromArgb(0xFF, 0x2F, 0x29, 0x9E);

            this.InitializeComponent();

            this.LoadApplication(new Core.App());
        }
    }
}
