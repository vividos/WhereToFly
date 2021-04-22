using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
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
