using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Tabbed page showing current position details and compass details
    /// </summary>
    public partial class CurrentPositionTabbedPage : TabbedPage
    {
        /// <summary>
        /// Creates new current position tabbed page
        /// </summary>
        public CurrentPositionTabbedPage()
        {
            this.InitializeComponent();

            this.Children.Add(new CurrentPositionDetailsPage());
            this.Children.Add(new CompassDetailsPage());
        }
    }
}
