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
        public Layer Layer { get; }

        #region Binding properties
        /// <summary>
        /// Property containing layer name
        /// </summary>
        public string Name
        {
            get
            {
                return this.Layer.Name;
            }
        }

        /// <summary>
        /// Returns image source for SvgImage in order to display the type image
        /// </summary>
        public ImageSource TypeImageSource { get; private set; }

        /// <summary>
        /// Command to execute when an item in the layer list has been tapped
        /// </summary>
        public AsyncCommand ItemTappedCommand { get; private set; }

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
            this.Layer = layer;

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.TypeImageSource = SvgImageCache.GetImageSource(this.Layer);
            this.VisibilityImageSource = SvgImageCache.GetLayerVisibilityImageSource(this.Layer);

            this.ItemTappedCommand = new AsyncCommand(this.OnTappedLayerItemAsync);
            this.VisibilityTappedCommand = new AsyncCommand(this.OnTappedLayerVisibilityAsync);
            this.ZoomToLayerContextAction = new AsyncCommand(this.OnZoomToLayerAsync);
            this.DeleteLayerContextAction = new AsyncCommand(this.OnDeleteLayerAsync, this.OnCanExecuteDeleteLayer);
        }

        /// <summary>
        /// Called when the user tapped on the layer item
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnTappedLayerItemAsync()
        {
            // due to a bug in Forms on Android, we have to solve tapping items using a gesture recognizer
            // see: https://github.com/xamarin/Xamarin.Forms/issues/2180
            if (Device.RuntimePlatform != Device.Android)
            {
                return;
            }

            await this.parentViewModel.ItemTappedCommand.ExecuteAsync(this.Layer);
        }

        /// <summary>
        /// Called when the user tapped on the layer visibility icon
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnTappedLayerVisibilityAsync()
        {
            this.Layer.IsVisible = !this.Layer.IsVisible;

            IDataService dataService = DependencyService.Get<IDataService>();
            var layerDataService = dataService.GetLayerDataService();
            await layerDataService.Update(this.Layer);

            App.MapView.SetLayerVisibility(this.Layer);

            this.VisibilityImageSource = SvgImageCache.GetLayerVisibilityImageSource(Layer);
            this.OnPropertyChanged(nameof(this.VisibilityImageSource));
        }

        /// <summary>
        /// Called when "zoom to" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToLayerAsync()
        {
            await this.parentViewModel.ZoomToLayer(this.Layer);
        }

        /// <summary>
        /// Called when "delete" context action is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteLayerAsync()
        {
            await this.parentViewModel.DeleteLayer(this.Layer);
        }

        /// <summary>
        /// Determines if a layer's "delete layer" context action can be executed. Prevents
        /// deleting default layers.
        /// </summary>
        /// <param name="arg">argument; unused</param>
        /// <returns>true when context action can be executed, false when not</returns>
        private bool OnCanExecuteDeleteLayer(object arg) =>
            this.Layer.LayerType != LayerType.LocationLayer &&
            this.Layer.LayerType != LayerType.TrackLayer;
    }
}
