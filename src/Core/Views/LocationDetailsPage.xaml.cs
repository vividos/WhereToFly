using WhereToFly.Core.ViewModels;
using WhereToFly.Logic.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.Core.Views
{
    /// <summary>
    /// Page to display location details
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationDetailsPage : ContentPage
    {
        /// <summary>
        /// Creates new location details page
        /// </summary>
        /// <param name="location">location to display</param>
        public LocationDetailsPage(Location location)
        {
            this.Title = "Location details";

            this.InitializeComponent();

            this.BindingContext = new LocationDetailsViewModel(App.Settings, location);
        }
    }
}
