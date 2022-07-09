using WhereToFly.App.Core.Services;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// The root page of the app, showing a slide-out menu on the left, and the map page in
    /// the center of the screen.
    /// </summary>
    public class RootPage : FlyoutPage
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
            this.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

            // set up flyout menu page
            this.Flyout = new MenuPage
            {
                BackgroundColor = Constants.PrimaryColor,
            };
            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Flyout.IconImageSource = ImageSource.FromFile("Assets/images/menu.png");
            }

            // set up detail page
            var mapPage = (App.Current as App).MapPage;
            var navigationPage = new NavigationPage(mapPage)
            {
                BarBackgroundColor = Constants.PrimaryColor,
                BarTextColor = Color.White,
            };

            NavigationPage.SetTitleIconImageSource(navigationPage, "icon.png");

            this.Detail = navigationPage;

            NavigationService.Instance.NavigationPage = navigationPage;
        }
    }
}
