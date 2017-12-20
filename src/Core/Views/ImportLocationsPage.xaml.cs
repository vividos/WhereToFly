using WhereToFly.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.Core.Views
{
    /// <summary>
    /// Page that shows some buttons to import location lists or to clear it
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImportLocationsPage : ContentPage
    {
        /// <summary>
        /// Creates new locations page
        /// </summary>
        public ImportLocationsPage()
        {
            this.InitializeComponent();

            this.BindingContext = new ImportLocationsViewModel();
        }
    }
}
