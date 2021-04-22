using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display track details
    /// </summary>
    public partial class TrackDetailsPage : ContentPage
    {
        /// <summary>
        /// Creates new track details page
        /// </summary>
        /// <param name="track">track to display</param>
        public TrackDetailsPage(Track track)
        {
            this.BindingContext = new TrackDetailsViewModel(track);
            this.InitializeComponent();
        }
    }
}
