using System.Windows.Input;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Logic;
using WhereToFly.App.Resources;
using WhereToFly.App.Services;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the layer details page
    /// </summary>
    public class LayerDetailsViewModel : ViewModelBase
    {
        /// <summary>
        /// Data service
        /// </summary>
        private readonly IDataService dataService;

        /// <summary>
        /// App map services
        /// </summary>
        private readonly IAppMapService appMapService;

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
                return Strings.ResourceManager.GetString(key) ?? "???";
            }
        }

        /// <summary>
        /// Property containing layer description web view source
        /// </summary>
        public WebViewSource? DescriptionWebViewSource
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
            this.dataService = Services.GetRequiredService<IDataService>();
            this.appMapService = Services.GetRequiredService<IAppMapService>();

            this.layer = layer;

            this.TypeImageSource =
                ImageSource.FromFile(
                    LayerListViewModel.ImagePathFromLayerType(layer.LayerType));

            this.DescriptionWebViewSource = new HtmlWebViewSource
            {
                Html = FormatLayerDescription(this.layer),
                BaseUrl = "about:blank",
            };

            this.ZoomToLayerCommand = new AsyncRelayCommand(
                this.OnZoomToLayerAsync,
                () => this.IsEnabledZoomToLayer);

            this.ExportLayerCommand = new AsyncRelayCommand(
                this.OnExportLayerAsync,
                () => this.IsEnabledExportLayer);

            this.DeleteLayerCommand = new AsyncRelayCommand(
                this.OnDeleteLayerAsync,
                () => this.IsEnabledDeleteLayer);
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
                App.GetResourceColor("ElementTextColor", true),
                App.GetResourceColor("PageBackgroundColor", true),
                App.GetResourceColor("AccentColor", true));
        }

        /// <summary>
        /// Called when "Zoom to" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnZoomToLayerAsync()
        {
            this.appMapService.MapView.ZoomToLayer(this.layer);

            await UserInterface.NavigationService.GoToMap();
        }

        /// <summary>
        /// Called when "Export" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnExportLayerAsync()
        {
            await ExportFileHelper.ExportLayerAsync(this.layer);
        }

        /// <summary>
        /// Called when "Delete" menu item is selected
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnDeleteLayerAsync()
        {
            var layerDataService = this.dataService.GetLayerDataService();

            await layerDataService.Remove(this.layer.Id);

            this.appMapService.MapView.RemoveLayer(this.layer);

            await UserInterface.NavigationService.GoBack();

            UserInterface.DisplayToast("Selected layer was deleted.");
        }
    }
}
