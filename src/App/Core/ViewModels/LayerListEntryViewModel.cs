using System.Threading.Tasks;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for a single layer list entry
    /// </summary>
    public class LayerListEntryViewModel
    {
        /// <summary>
        /// Parent view model
        /// </summary>
        private readonly LayerListViewModel parentViewModel;

        /// <summary>
        /// Layer to show
        /// </summary>
        private readonly Layer layer;

        #region Binding properties
        /// <summary>
        /// Property containing layer name
        /// </summary>
        public string Name
        {
            get
            {
                return this.layer.Name;
            }
        }

        /// <summary>
        /// Returns image source for SvgImage in order to display the type image
        /// </summary>
        public ImageSource TypeImageSource { get; }

        /// <summary>
        /// Returns image source for SvgImage in order to display the visibility of the layer
        /// </summary>
        public ImageSource VisibilityImageSource { get; }

        /// <summary>
        /// Command to execute when "zoom to" context action is selected on a layer
        /// </summary>
        public Command ZoomToLayerContextAction { get; set; }

        /// <summary>
        /// Command to execute when "delete" context action is selected on a layer
        /// </summary>
        public Command DeleteLayerContextAction { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model object based on the given layer object
        /// </summary>
        /// <param name="parentViewModel">parent view model</param>
        /// <param name="layer">layer object</param>
        public LayerListEntryViewModel(LayerListViewModel parentViewModel, Layer layer)
        {
            this.parentViewModel = parentViewModel;
            this.layer = layer;

            this.TypeImageSource = SvgImageCache.GetImageSource(layer, "#000000");
            this.VisibilityImageSource = SvgImageCache.GetLayerVisibilityImageSource(layer, "#000000");

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.ZoomToLayerContextAction =
                new Command(async () => await this.OnZoomToLayerAsync());

            this.DeleteLayerContextAction =
                new Command(async () => await this.OnDeleteLayerAsync());
        }

        /// <summary>
        /// Called when "zoom to" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToLayerAsync()
        {
            await this.parentViewModel.ZoomToLayer(this.layer);
        }

        /// <summary>
        /// Called when "delete" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteLayerAsync()
        {
            await this.parentViewModel.DeleteLayer(this.layer);
        }
    }
}
