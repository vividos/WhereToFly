using System.Windows.Input;
using WhereToFly.App.Logic;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.ViewModels
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
        public ICommand ItemTappedCommand { get; private set; }

        /// <summary>
        /// Returns image source for SvgImage in order to display the visibility of the layer
        /// </summary>
        public ImageSource VisibilityImageSource { get; private set; }

        /// <summary>
        /// Returns if the layer can be zoomed to
        /// </summary>
        public bool IsEnabledZoomToLayer =>
            this.Layer.LayerType != LayerType.OsmBuildingsLayer;

        /// <summary>
        /// Returns if the layer can be exported
        /// </summary>
        public bool IsEnabledExportLayer =>
            this.Layer.LayerType != LayerType.LocationLayer &&
            this.Layer.LayerType != LayerType.TrackLayer &&
            this.Layer.LayerType != LayerType.OsmBuildingsLayer;

        /// <summary>
        /// Returns if the layer can be deleted
        /// </summary>
        public bool IsEnabledDeleteLayer =>
            this.Layer.LayerType != LayerType.LocationLayer &&
            this.Layer.LayerType != LayerType.TrackLayer;

        /// <summary>
        /// Command to execute when user tapped on the layer visibility icon
        /// </summary>
        public ICommand VisibilityTappedCommand { get; private set; }

        /// <summary>
        /// Command to execute when "zoom to" context menu item is selected on a layer
        /// </summary>
        public ICommand ZoomToLayerCommand { get; private set; }

        /// <summary>
        /// Command to execute when "Export" context menu item is selected on a layer
        /// </summary>
        public ICommand ExportLayerCommand { get; set; }

        /// <summary>
        /// Command to execute when "delete" context menu item is selected on a layer
        /// </summary>
        public ICommand DeleteLayerCommand { get; private set; }
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

            this.TypeImageSource = SvgImageCache.GetImageSource(this.Layer);
            this.VisibilityImageSource = SvgImageCache.GetLayerVisibilityImageSource(this.Layer);

            this.ItemTappedCommand = new AsyncRelayCommand(this.OnTappedLayerItemAsync);
            this.VisibilityTappedCommand = new AsyncRelayCommand(this.OnTappedLayerVisibilityAsync);

            this.ZoomToLayerCommand = new AsyncRelayCommand(
                this.OnZoomToLayerAsync,
                () => this.IsEnabledZoomToLayer);

            this.ExportLayerCommand = new AsyncRelayCommand(
                this.OnExportLayerAsync,
                () => this.IsEnabledExportLayer);

            this.DeleteLayerCommand = new AsyncRelayCommand(
                this.OnDeleteLayerAsync,
                () =>
                this.Layer.LayerType != LayerType.LocationLayer &&
                this.Layer.LayerType != LayerType.TrackLayer);
        }

        /// <summary>
        /// Called when the user tapped on the layer item
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnTappedLayerItemAsync()
        {
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

            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.MapView.SetLayerVisibility(this.Layer);

            this.VisibilityImageSource = SvgImageCache.GetLayerVisibilityImageSource(this.Layer);
            this.OnPropertyChanged(nameof(this.VisibilityImageSource));
        }

        /// <summary>
        /// Called when "zoom to" context menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToLayerAsync()
        {
            await this.parentViewModel.ZoomToLayer(this.Layer);
        }

        /// <summary>
        /// Called when "Export" context menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnExportLayerAsync()
        {
            await this.parentViewModel.ExportLayer(this.Layer);
        }

        /// <summary>
        /// Called when "delete" context menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteLayerAsync()
        {
            await this.parentViewModel.DeleteLayer(this.Layer);
        }
    }
}
