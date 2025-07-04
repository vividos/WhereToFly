﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using WhereToFly.App.Logic;
using WhereToFly.App.Services;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.ViewModels
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
            { "vividos' Where-to-fly resources", "https://www.vividos.de/wheretofly/index.html" },
            { "XContest Airspaces", "https://airspace.xcontest.org/" },
            { "DHV-XC Lufträume", "https://www.dhv-xc.de/xc/modules/leonardo/index.php?name=leonardo&op=luftraum" },
        };

        /// <summary>
        /// Layer list backing store
        /// </summary>
        private List<Layer> layerList = [];

        #region Binding properties
        /// <summary>
        /// Location list view models to display
        /// </summary>
        public ObservableCollection<LayerListEntryViewModel>? LayerList { get; set; }

        /// <summary>
        /// Indicates if the layer list is empty. Default layers like location and track layer are
        /// disregarded.
        /// </summary>
        public bool IsListEmpty => this.LayerList == null || !this.LayerList.Any();

        /// <summary>
        /// Stores the selected layer when an item is tapped
        /// </summary>
        public LayerListEntryViewModel? SelectedLayer { get; set; }

        /// <summary>
        /// Command to execute when an item in the layer list has been tapped
        /// </summary>
        public AsyncRelayCommand<Layer> ItemTappedCommand { get; private set; }

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
        public AsyncRelayCommand DeleteLayerListCommand { get; private set; }
        #endregion

        /// <summary>
        /// Creates a new view model object for the layer list page
        /// </summary>
        public LayerListViewModel()
        {
            Task.Run(this.LoadDataAsync);

            this.ItemTappedCommand =
                new AsyncRelayCommand<Layer>(this.NavigateToLayerDetails);

            this.ImportLayerCommand = new AsyncRelayCommand(this.ImportLayerAsync);

            this.DeleteLayerListCommand =
                new AsyncRelayCommand(
                    this.ClearLayersAsync,
                    () => this.IsClearLayerListEnabled);
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
                await dataService.InitCompleteTask;

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
                MainThread.BeginInvokeOnMainThread(this.DeleteLayerListCommand.NotifyCanExecuteChanged);
            });
        }

        /// <summary>
        /// Returns to map view and zooms to the given layer
        /// </summary>
        /// <param name="layer">layer to zoom to</param>
        /// <returns>task to wait on</returns>
        internal async Task ZoomToLayer(Layer layer)
        {
            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.MapView.ZoomToLayer(layer);

            await NavigationService.GoToMap();
        }

        /// <summary>
        /// Navigates to layer details page
        /// </summary>
        /// <param name="layer">layer to show</param>
        /// <returns>task to wait on</returns>
        private async Task NavigateToLayerDetails(Layer? layer)
        {
            this.SelectedLayer = null;
            this.OnPropertyChanged(nameof(this.SelectedLayer));

            if (layer != null &&
                layer.LayerType != LayerType.LocationLayer &&
                layer.LayerType != LayerType.TrackLayer)
            {
                await NavigationService.Instance.NavigateAsync(PageKey.LayerDetailsPage, true, layer);
            }
            else
            {
                UserInterface.DisplayToast("No details for this layer available.");
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

            string result = await UserInterface.DisplayActionSheet(
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
                            { DevicePlatform.Android, new string[] { } },
                            { DevicePlatform.WinUI, new string[] { ".czml" } },
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

                await UserInterface.DisplayAlert(
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
                            { DevicePlatform.WinUI, new string[] { ".txt" } },
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

                await UserInterface.DisplayAlert(
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

            var appMapService = DependencyService.Get<IAppMapService>();
            await appMapService.MapView.AddLayer(layer);

            UserInterface.DisplayToast("Layer was added.");
        }

        /// <summary>
        /// Presents a list of websites to download from and opens selected URL. Importing is then
        /// done using the file extension association.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task DownloadFromWebAsync()
        {
            string result = await UserInterface.DisplayActionSheet(
                "Select a web page to open",
                "Cancel",
                null,
                this.downloadWebSiteList.Keys.ToArray());

            if (result == null ||
                !this.downloadWebSiteList.TryGetValue(result, out string? webSiteToOpen))
            {
                return;
            }

            await Browser.OpenAsync(
                webSiteToOpen,
                BrowserLaunchMode.External);
        }

        /// <summary>
        /// Clears all layers
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task ClearLayersAsync()
        {
            bool result = await UserInterface.DisplayAlert(
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

            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.MapView.ClearLayerList();

            UserInterface.DisplayToast("Layer list was cleared.");
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

            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.MapView.RemoveLayer(layer);

            UserInterface.DisplayToast("Selected layer was deleted.");
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
