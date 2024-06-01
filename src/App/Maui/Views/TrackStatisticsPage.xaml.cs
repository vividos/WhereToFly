using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Views
{
    /// <summary>
    /// Page to display track statistics
    /// </summary>
    public partial class TrackStatisticsPage : ContentPage
    {
        /// <summary>
        /// Creates new track statistics page
        /// </summary>
        /// <param name="track">track to display</param>
        public TrackStatisticsPage(Track track)
        {
            this.BindingContext = new TrackStatisticsViewModel(track);
            this.InitializeComponent();
        }
    }
}
