using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display layer list; the layers' visibility can be set, and a layer can be zoomed
    /// to.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LayerListPage : ContentPage
    {
        /// <summary>
        /// Creates a new layer list page
        /// </summary>
        public LayerListPage()
        {
            this.Title = "Layer list";

            this.InitializeComponent();

            this.BindingContext = new LayerListViewModel();
        }
    }
}
