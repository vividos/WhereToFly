using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Tabbed page showing several tabs with track details
    /// </summary>
    public partial class TrackInfoTabbedPage : TabbedPage
    {
        /// <summary>
        /// Creates new track info tabbed page
        /// </summary>
        /// <param name="track">track to display</param>
        public TrackInfoTabbedPage(Track track)
        {
            this.BindingContext = new TrackTabViewModel(track);
            this.InitializeComponent();

            this.Children.Add(new TrackDetailsPage(track));
            this.Children.Add(new TrackStatisticsPage(track));
        }
    }
}
