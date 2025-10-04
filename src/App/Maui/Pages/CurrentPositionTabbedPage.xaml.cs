namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Tabbed page showing current position details and compass details
    /// </summary>
    public partial class CurrentPositionTabbedPage : TabbedPage
    {
        /// <summary>
        /// Creates new current position tabbed page
        /// </summary>
        /// <param name="services">service provider</param>
        public CurrentPositionTabbedPage(IServiceProvider services)
        {
            this.InitializeComponent();

            this.Children.Add(new CurrentPositionDetailsPage(services));
            this.Children.Add(new CompassDetailsPage(services));
        }
    }
}
