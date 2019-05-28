using WhereToFly.App.Core.Services;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// The root page of the app, showing a slide-out menu on the left, and the map page in
    /// the center of the screen.
    /// </summary>
    public class RootPage : MasterDetailPage
    {
        /// <summary>
        /// Creates a new root page
        /// </summary>
        public RootPage()
        {
            this.InitLayout();
        }

        /// <summary>
        /// Initializes layout of page
        /// </summary>
        private void InitLayout()
        {
            this.MasterBehavior = MasterBehavior.Popover;

            // set up master page
            this.Master = new MenuPage();
            this.Master.BackgroundColor = Constants.PrimaryColor;

            // set up detail page
            var mapPage = (App.Current as App).MapPage;
            var navigationPage = new NavigationPage(mapPage);

            navigationPage.BarBackgroundColor = Constants.PrimaryColor;
            navigationPage.BarTextColor = Color.White;

            NavigationPage.SetTitleIconImageSource(navigationPage, "icon.png");

            this.Detail = navigationPage;

            NavigationService.Instance.NavigationPage = navigationPage;
        }
    }
}
