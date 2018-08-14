using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Geo;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display track details
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
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
