using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public Command ImportLayerCommand { get; set; }

        /// <summary>
        /// Command to execute when toolbar button "delete layer list" has been tapped
        /// </summary>
        public Command DeleteLayerListCommand { get; private set; }
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

            this.ImportLayerCommand =
                new Command(async () => await this.ImportLayerAsync());

            this.DeleteLayerListCommand =
                new Command(
                    async () =>
                    {
                        await this.ClearLayersAsync();
                    },
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

                this.layerList = await dataService.GetLayerListAsync(CancellationToken.None);
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
                App.RunOnUiThread(this.DeleteLayerListCommand.ChangeCanExecute);
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
            await dataService.StoreLayerListAsync(new List<Layer>());

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
            await dataService.StoreLayerListAsync(this.layerList);

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
