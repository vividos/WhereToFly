using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Logic;
using WhereToFly.App.Core.Resources;
using WhereToFly.App.Core.Services;
using WhereToFly.Geo.Model;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the layer details page
    /// </summary>
    public class LayerDetailsViewModel : ViewModelBase
    {
        /// <summary>
        /// Layer to show
        /// </summary>
        private readonly Layer layer;

        #region Binding properties
        /// <summary>
        /// Property containing layer name
        /// </summary>
        public string Name => this.layer.Name;

        /// <summary>
        /// Returns image source for SvgImage in order to display the type image
        /// </summary>
        public ImageSource TypeImageSource { get; }

        /// <summary>
        /// Property containing layer type
        /// </summary>
        public string Type
        {
            get
            {
                string key = $"LayerType_{this.layer.LayerType}";
                return Strings.ResourceManager.GetString(key);
            }
        }

        /// <summary>
        /// Property containing layer description web view source
        /// </summary>
        public WebViewSource DescriptionWebViewSource
        {
            get; private set;
        }

        /// <summary>
        /// Returns if the layer can be zoomed to
        /// </summary>
        public bool IsEnabledZoomToLayer =>
            this.layer.LayerType != LayerType.OsmBuildingsLayer;

        /// <summary>
        /// Returns if the layer can be exported
        /// </summary>
        public bool IsEnabledExportLayer =>
            this.layer.LayerType != LayerType.LocationLayer &&
            this.layer.LayerType != LayerType.TrackLayer &&
            this.layer.LayerType != LayerType.OsmBuildingsLayer;

        /// <summary>
        /// Returns if the layer can be deleted
        /// </summary>
        public bool IsEnabledDeleteLayer =>
            this.layer.LayerType != LayerType.LocationLayer &&
            this.layer.LayerType != LayerType.TrackLayer;

        /// <summary>
        /// Command to execute when "zoom to" menu item is selected on a layer
        /// </summary>
        public ICommand ZoomToLayerCommand { get; set; }

        /// <summary>
        /// Command to execute when "export" menu item is selected on a layer
        /// </summary>
        public ICommand ExportLayerCommand { get; set; }

        /// <summary>
        /// Command to execute when "delete" menu item is selected on a layer
        /// </summary>
        public ICommand DeleteLayerCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model object based on the given layer object
        /// </summary>
        /// <param name="layer">layer object</param>
        public LayerDetailsViewModel(Layer layer)
        {
            this.layer = layer;

            this.TypeImageSource = SvgImageCache.GetImageSource(layer);

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings for this view model
        /// </summary>
        private void SetupBindings()
        {
            this.DescriptionWebViewSource = new HtmlWebViewSource
            {
                Html = FormatLayerDescription(this.layer),
                BaseUrl = "about:blank",
            };

            this.ZoomToLayerCommand = new AsyncCommand(this.OnZoomToLayerAsync, this.OnCanExecuteZoomToLayer);
            this.ExportLayerCommand = new AsyncCommand(this.OnExportLayerAsync, this.OnCanExecuteExportLayer);
            this.DeleteLayerCommand = new AsyncCommand(this.OnDeleteLayerAsync, this.OnCanExecuteDeleteLayer);
        }

        /// <summary>
        /// Formats layer description
        /// </summary>
        /// <param name="layer">layer to format description</param>
        /// <returns>formatted description text</returns>
        private static string FormatLayerDescription(Layer layer)
        {
            string desc = HtmlConverter.FromHtmlOrMarkdown(layer.Description);

            return HtmlConverter.AddTextColorStyles(
                desc,
                App.GetResourceColor("ElementTextColor"),
                App.GetResourceColor("PageBackgroundColor"),
                App.GetResourceColor("AccentColor"));
        }

        /// <summary>
        /// Called when "Zoom to" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToLayerAsync()
        {
            App.MapView.ZoomToLayer(this.layer);

            await NavigationService.GoToMap();
        }

        /// <summary>
        /// Determines if a layer's "zoom to layer" toolbar action can be executed. Prevents
        /// zooming to OSM buildings layer.
        /// </summary>
        /// <param name="arg">argument; unused</param>
        /// <returns>true when context action can be executed, false when not</returns>
        private bool OnCanExecuteZoomToLayer(object arg) => this.IsEnabledZoomToLayer;

        /// <summary>
        /// Called when "Export" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnExportLayerAsync()
        {
            await ExportFileHelper.ExportLayerAsync(this.layer);
        }

        /// <summary>
        /// Determines if a layer's "export layer" toolbar action can be executed. Prevents
        /// exporting layers that can't be exported.
        /// </summary>
        /// <param name="arg">argument; unused</param>
        /// <returns>true when context action can be executed, false when not</returns>
        private bool OnCanExecuteExportLayer(object arg) => this.IsEnabledExportLayer;

        /// <summary>
        /// Called when "Delete" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteLayerAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            var layerDataService = dataService.GetLayerDataService();

            await layerDataService.Remove(this.layer.Id);

            App.MapView.RemoveLayer(this.layer);

            await NavigationService.Instance.GoBack();

            App.ShowToast("Selected layer was deleted.");
        }

        /// <summary>
        /// Determines if a layer's "delete" toolbar action can be executed. Prevents
        /// exporting layers that can't be deleted.
        /// </summary>
        /// <param name="arg">argument; unused</param>
        /// <returns>true when context action can be executed, false when not</returns>
        private bool OnCanExecuteDeleteLayer(object arg) => this.IsEnabledDeleteLayer;
    }
}
