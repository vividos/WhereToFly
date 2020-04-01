using MvvmHelpers.Commands;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Services;
using WhereToFly.Shared.Model;
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
        private readonly Dictionary<string, string> downloadWebSiteList = new Dictionary<string, string>
        {
            { "vividos' Where-to-fly resources", "http://www.vividos.de/wheretofly/index.html" },
            { "XContest Airspaces", "https://airspace.xcontest.org/" },
            { "OpenAir Schutzzonen", "https://www.openairschutzzonen.de/" },
            { "Flyland.ch Lufträume", "http://www.flyland.ch/download.php" }
        };

        /// <summary>
        /// Layer list backing store
        /// </summary>
        private List<Layer> layerList = new List<Layer>();

        #region Binding properties
        /// <summary>
        /// Location list view models to display
        /// </summary>
        public ObservableCollection<LayerListEntryViewModel> LayerList { get; set; }

        /// <summary>
        /// Indicates if the layer list is empty.
        /// </summary>
        public bool IsListEmpty
        {
            get => this.LayerList == null || !this.LayerList.Any();
        }

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

                this.layerList = (await layerDataService.GetList()).ToList();
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

            await NavigationService.Instance.NavigateAsync(Constants.PageKeyMapPage, animated: true);
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
                "Download from web"
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
            FileData result;
            try
            {
                string[] fileTypes = null;

                if (Device.RuntimePlatform == Device.UWP)
                {
                    fileTypes = new string[] { ".czml" };
                }

                result = await CrossFilePicker.Current.PickFile(fileTypes);
                if (result == null ||
                    string.IsNullOrEmpty(result.FilePath))
                {
                    return;
                }
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

            using (var stream = result.GetStream())
            {
                await OpenFileHelper.OpenLayerFileAsync(stream, result.FileName);
            }

            await this.ReloadLayerListAsync();
        }

        /// <summary>
        /// Imports OpenAir airspaces file and adds layer
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ImportOpenAirAirspacesAsync()
        {
            FileData result;
            try
            {
                string[] fileTypes = null;

                if (Device.RuntimePlatform == Device.UWP)
                {
                    fileTypes = new string[] { ".txt" };
                }
                else if (Device.RuntimePlatform == Device.Android)
                {
                    fileTypes = new string[] { "text/plain" };
                }

                result = await CrossFilePicker.Current.PickFile(fileTypes);
                if (result == null ||
                    string.IsNullOrEmpty(result.FilePath))
                {
                    return;
                }
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

            using (var stream = result.GetStream())
            {
                await OpenFileHelper.ImportOpenAirAirspaceFile(stream, result.FileName);
            }

            await this.ReloadLayerListAsync();
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

            await Xamarin.Essentials.Launcher.OpenAsync(webSiteToOpen);
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

            await this.ReloadLayerListAsync();

            App.MapView.ClearLayerList();

            App.ShowToast("Layer list was cleared.");
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
