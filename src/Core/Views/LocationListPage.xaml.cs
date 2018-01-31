using WhereToFly.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.Core.Views
{
    /// <summary>
    /// Page to display location list; the list can be filtered by a filter text, and a single
    /// location entry can be tapped to get more infos.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationListPage : ContentPage
    {
        /// <summary>
        /// Creates a new location list page
        /// </summary>
        public LocationListPage()
        {
            this.Title = "Location list";

            this.InitializeComponent();

            this.BindingContext = new LocationListViewModel();
        }
    }
}
