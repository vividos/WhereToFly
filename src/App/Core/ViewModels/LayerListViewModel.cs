using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Services;
using WhereToFly.Geo.Model;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the layer list page
    /// </summary>
    public class LayerListViewModel : ViewModelBase
    {
        /// <summary>
        /// A mapping of display string to website address to open
        /// </summary>
        private readonly Dictionary<string, string> downloadWebSiteList = new()
        {
            { "vividos' Where-to-fly resources", "http://www.vividos.de/wheretofly/index.html" },
            { "XContest Airspaces", "https://airspace.xcontest.org/" },
            { "DHV-XC Lufträume", "https://www.dhv-xc.de/xc/modules/leonardo/index.php?name=leonardo&op=luftraum" },
            { "OpenAir Schutzzonen", "https://www.openairschutzzonen.de/" },
            { "Flyland.ch Lufträume", "http://www.flyland.ch/download.php" },
        };

        /// <summary>
        /// Layer list backing store
        /// </summary>
        private List<Layer> layerList = new();

        #region Binding properties
        /// <summary>
        /// Location list view models to display
        /// </summary>
        public ObservableCollection<LayerListEntryViewModel> LayerList { get; set; }

        /// <summary>
        /// Indicates if the layer list is empty. Default layers like location and track layer are
        /// disregarded.
        /// </summary>
        public bool IsListEmpty => this.LayerList == null || !this.LayerList.Any();

        /// <summary>
        /// Stores the selected layer when an item is tapped
        /// </summary>
        public LayerListEntryViewModel SelectedLayer { get; set; }

        /// <summary>
        /// Command to execute when an item in the layer list has been tapped
        /// </summary>
        public AsyncCommand<Layer> ItemTappedCommand { get; private set; }

        /// <summary>
        /// Indicates if the "clear layer list" button is enabled.
        /// </summary>
        public bool IsClearLayerListEnabled => !this.IsListEmpty;

        /// <summary>
        /// Command to execute when toolbar button "import layer" has been tapped
        /// </summary>
        public ICommand ImportLayerCommand { get; set; }

        /// <summary>
        /// Command to execute when toolbar button "delete layer list" has been tapped
        /// </summary>
        public AsyncCommand DeleteLayerListCommand { get; private set; }
        #endregion

        /// <summary>
        /// Creates a new view model object for the layer list page
        /// </summary>
        public LayerListViewModel()
        {
            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            Task.Run(this.LoadDataAsync);

            this.ItemTappedCommand =
                new AsyncCommand<Layer>(this.NavigateToLayerDetails);

            this.ImportLayerCommand = new AsyncCommand(this.ImportLayerAsync);

            this.DeleteLayerListCommand =
                new AsyncCommand(
                    this.ClearLayersAsync,
                    (obj) => this.IsClearLayerListEnabled);
        }

        /// <summary>
        /// Loads data; async method
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task LoadDataAsync()
        {
            try
            {
                IDataService dataService = DependencyService.Get<IDataService>();
                var layerDataService = dataService.GetLayerDataService();

                var localLayerList = await layerDataService.GetList();
                this.layerList = localLayerList.ToList();
            }
            catch (Exception ex)
            {
                App.LogError(ex);
            }

            this.UpdateLayerList();
        }

        /// <summary>
        /// Updates layer list
        /// </summary>
        private void UpdateLayerList()
        {
            Task.Run(() =>
            {
                var newList = this.layerList
                    .Select(layer => new LayerListEntryViewModel(this, layer));

                this.LayerList = new ObservableCollection<LayerListEntryViewModel>(newList);

                this.OnPropertyChanged(nameof(this.LayerList));
                this.OnPropertyChanged(nameof(this.IsListEmpty));
                this.OnPropertyChanged(nameof(this.IsClearLayerListEnabled));
                App.RunOnUiThread(this.DeleteLayerListCommand.RaiseCanExecuteChanged);
            });
        }

        /// <summary>
        /// Returns to map view and zooms to the given layer
        /// </summary>
        /// <param name="layer">layer to zoom to</param>
        /// <returns>task to wait on</returns>
        internal async Task ZoomToLayer(Layer layer)
        {
            App.MapView.ZoomToLayer(layer);

            await NavigationService.GoToMap();
        }

        /// <summary>
        /// Navigates to layer details page
        /// </summary>
        /// <param name="layer">layer to show</param>
        /// <returns>task to wait on</returns>
        private async Task NavigateToLayerDetails(Layer layer)
        {
            this.SelectedLayer = null;
            this.OnPropertyChanged(nameof(this.SelectedLayer));

            if (layer.LayerType != LayerType.LocationLayer &&
                layer.LayerType != LayerType.TrackLayer)
            {
                await NavigationService.Instance.NavigateAsync(PageKey.LayerDetailsPage, true, layer);
            }
            else
            {
                App.ShowToast("No details for this layer available.");
            }
        }

        /// <summary>
        /// Shows menu to import layer
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportLayerAsync()
        {
            var importActions = new List<string>
            {
                "Import CZML Layer",
                "Import OpenAir airspaces",
                "Add OpenStreetMap Buildings Layer",
                "Download from web",
            };

            string result = await App.Current.MainPage.DisplayActionSheet(
                $"Import layer",
                "Cancel",
                null,
                importActions.ToArray());

            if (!string.IsNullOrEmpty(result))
            {
                int selectedIndex = importActions.IndexOf(result);

                switch (selectedIndex)
                {
                    case 0:
                        await this.ImportCzmlLayerAsync();
                        break;

                    case 1:
                        await this.ImportOpenAirAirspacesAsync();
                        break;

                    case 2:
                        await this.AddOpenStreetMapLayerAsync();
                        break;

                    case 3:
                        await this.DownloadFromWebAsync();
                        break;

                    default:
                        // ignore
                        break;
                }
            }
        }

        /// <summary>
        /// Imports a CZML layer by showing file picker and opening the layer file
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportCzmlLayerAsync()
        {
            try
            {
                var options = new PickOptions
                {
                    FileTypes = new FilePickerFileType(
                        new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.Android, null },
                            { DevicePlatform.UWP, new string[] { ".czml" } },
                            { DevicePlatform.iOS, null },
                        }),
                    PickerTitle = "Select a CZML layer file to import",
                };

                var result = await FilePicker.PickAsync(options);
                if (result == null ||
                    string.IsNullOrEmpty(result.FullPath))
                {
                    return;
                }

                using var stream = await result.OpenReadAsync();
                await OpenFileHelper.OpenLayerFileAsync(stream, result.FileName);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while picking a file: " + ex.Message,
                    "OK");

                return;
            }

            await this.ReloadLayerListAsync();
        }

        /// <summary>
        /// Imports OpenAir airspaces file and adds layer
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportOpenAirAirspacesAsync()
        {
            try
            {
                var options = new PickOptions
                {
                    FileTypes = new FilePickerFileType(
                        new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.Android, new string[] { "text/plain" } },
                            { DevicePlatform.UWP, new string[] { ".txt" } },
                            { DevicePlatform.iOS, null },
                        }),
                    PickerTitle = "Select an OpenAir text file to import",
                };

                var result = await FilePicker.PickAsync(options);
                if (result == null ||
                    string.IsNullOrEmpty(result.FullPath))
                {
                    return;
                }

                using var stream = await result.OpenReadAsync();
                await OpenFileHelper.ImportOpenAirAirspaceFile(stream, result.FileName);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while picking a file: " + ex.Message,
                    "OK");

                return;
            }

            await this.ReloadLayerListAsync();
        }

        /// <summary>
        /// Adds (or re-adds at the end) the Cesium OpenStreetMap Buildings layer
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task AddOpenStreetMapLayerAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            var layerDataService = dataService.GetLayerDataService();

            var layer = DataServiceHelper.GetOpenStreetMapBuildingsLayer();

            await layerDataService.Remove(layer.Id);
            await layerDataService.Add(layer);

            await NavigationService.GoToMap();

            App.MapView.AddLayer(layer);

            App.ShowToast("Layer was added.");
        }

        /// <summary>
        /// Presents a list of websites to download from and opens selected URL. Importing is then
        /// done using the file extension association.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task DownloadFromWebAsync()
        {
            string result = await App.Current.MainPage.DisplayActionSheet(
                "Select a web page to open",
                "Cancel",
                null,
                this.downloadWebSiteList.Keys.ToArray());

            if (result == null ||
                !this.downloadWebSiteList.ContainsKey(result))
            {
                return;
            }

            string webSiteToOpen = this.downloadWebSiteList[result];

            await Browser.OpenAsync(webSiteToOpen, BrowserLaunchMode.External);
        }

        /// <summary>
        /// Clears all layers
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ClearLayersAsync()
        {
            bool result = await App.Current.MainPage.DisplayAlert(
                Constants.AppTitle,
                "Really clear all layers?",
                "Clear",
                "Cancel");

            if (!result)
            {
                return;
            }

            var dataService = DependencyService.Get<IDataService>();
            var layerDataService = dataService.GetLayerDataService();

            await layerDataService.ClearList();

            var defaultLayerList = DataServiceHelper.GetDefaultLayerList();
            await layerDataService.AddList(defaultLayerList);

            await this.ReloadLayerListAsync();

            App.MapView.ClearLayerList();

            App.ShowToast("Layer list was cleared.");
        }

        /// <summary>
        /// Called when "Export" menu item is selected
        /// </summary>
        /// <param name="layer">layer to export</param>
        /// <returns>task to wait on</returns>
        internal async Task ExportLayer(Layer layer)
        {
            await ExportFileHelper.ExportLayerAsync(layer);
        }

        /// <summary>
        /// Deletes the given layer from the layer list
        /// </summary>
        /// <param name="layer">layer to delete</param>
        /// <returns>task to wait on</returns>
        internal async Task DeleteLayer(Layer layer)
        {
            this.layerList.Remove(layer);

            var dataService = DependencyService.Get<IDataService>();
            var layerDataService = dataService.GetLayerDataService();

            await layerDataService.Remove(layer.Id);

            this.UpdateLayerList();

            App.MapView.RemoveLayer(layer);

            App.ShowToast("Selected layer was deleted.");
        }

        /// <summary>
        /// Reloads layer list and shows it on the page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ReloadLayerListAsync()
        {
            await this.LoadDataAsync();
            this.UpdateLayerList();
        }
    }
}
