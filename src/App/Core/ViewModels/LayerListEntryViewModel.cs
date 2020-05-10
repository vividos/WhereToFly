using MvvmHelpers.Commands;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for a single layer list entry
    /// </summary>
    public class LayerListEntryViewModel : ViewModelBase
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
        public ImageSource TypeImageSource { get; private set; }

        /// <summary>
        /// Returns image source for SvgImage in order to display the visibility of the layer
        /// </summary>
        public ImageSource VisibilityImageSource { get; private set; }

        /// <summary>
        /// Command to execute when user tapped on the layer visibility icon
        /// </summary>
        public ICommand VisibilityTappedCommand { get; private set; }

        /// <summary>
        /// Command to execute when "zoom to" context action is selected on a layer
        /// </summary>
        public ICommand ZoomToLayerContextAction { get; private set; }

        /// <summary>
        /// Command to execute when "delete" context action is selected on a layer
        /// </summary>
        public ICommand DeleteLayerContextAction { get; private set; }
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

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.TypeImageSource = SvgImageCache.GetImageSource(layer, "#000000");
            this.VisibilityImageSource = SvgImageCache.GetLayerVisibilityImageSource(layer, "#000000");

            this.VisibilityTappedCommand = new AsyncCommand(this.OnTappedLayerVisibilityAsync);
            this.ZoomToLayerContextAction = new AsyncCommand(this.OnZoomToLayerAsync);
            this.DeleteLayerContextAction = new AsyncCommand(this.OnDeleteLayerAsync, this.OnCanExecuteDeleteLayer);
        }

        /// <summary>
        /// Called when the user tapped on the layer visibility icon
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnTappedLayerVisibilityAsync()
        {
            this.layer.IsVisible = !this.layer.IsVisible;

            IDataService dataService = DependencyService.Get<IDataService>();
            var layerDataService = dataService.GetLayerDataService();
            await layerDataService.Update(this.layer);

            App.MapView.SetLayerVisibility(this.layer);

            this.VisibilityImageSource = SvgImageCache.GetLayerVisibilityImageSource(layer, "#000000");
            this.OnPropertyChanged(nameof(this.VisibilityImageSource));
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

        /// <summary>
        /// Determines if a layer's "delete layer" context action can be executed. Prevents
        /// deleting default layers.
        /// </summary>
        /// <param name="arg">argument; unused</param>
        /// <returns>true when context action can be executed, false when not</returns>
        private bool OnCanExecuteDeleteLayer(object arg) =>
            this.layer.LayerType != LayerType.LocationLayer &&
            this.layer.LayerType != LayerType.TrackLayer;
    }
}
