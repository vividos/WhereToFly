using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Page to display layer details
    /// </summary>
    public partial class LayerDetailsPage : ContentPage
    {
        /// <summary>
        /// Creates new layer details page
        /// </summary>
        /// <param name="layer">layer to display</param>
        public LayerDetailsPage(Layer layer)
        {
            this.BindingContext = new LayerDetailsViewModel(layer);

            this.InitializeComponent();
        }
    }
}
