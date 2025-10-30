using WhereToFly.App.Abstractions;
using WhereToFly.App.Services;

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// The root page of the app, showing a slide-out menu on the left, and the map page in
    /// the center of the screen.
    /// </summary>
    public class RootPage : FlyoutPage
    {
        /// <summary>
        /// Map page
        /// </summary>
        public MapPage MapPage { get; private set; }

        /// <summary>
        /// Creates a new root page
        /// </summary>
        public RootPage()
        {
            this.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

            // set up flyout menu page
            this.Flyout = new MenuPage
            {
                BackgroundColor = Constants.PrimaryColor,
            };

            // set up detail page
            this.MapPage = new MapPage();
            var navigationPage = new NavigationPage(this.MapPage)
            {
                BarBackgroundColor = Constants.PrimaryColor,
                BarTextColor = Colors.White,
            };

            NavigationPage.SetTitleIconImageSource(navigationPage, "icon.png");

            this.Detail = navigationPage;

            var navigationService = App.Services.GetRequiredService<INavigationService>();
            if (navigationService is NavigationService navigationServiceImpl)
            {
                navigationServiceImpl.NavigationPage = navigationPage;
            }
        }
    }
}
