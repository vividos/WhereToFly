using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Geo;
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
            this.Title = "Track details";

            this.InitializeComponent();

            this.BindingContext = new TrackDetailsViewModel(track);
        }
    }
}
