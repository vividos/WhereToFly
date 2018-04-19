using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Logic.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to edit location details, such as name, type, internet link and description text.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditLocationDetailsPage : ContentPage
    {
        /// <summary>
        /// Creates new edit location details page
        /// </summary>
        /// <param name="location">location object to edit</param>
        public EditLocationDetailsPage(Location location)
        {
            this.Title = "Edit location";

            this.InitializeComponent();

            this.BindingContext = new EditLocationDetailsViewModel(App.Settings, location);
        }
    }
}
