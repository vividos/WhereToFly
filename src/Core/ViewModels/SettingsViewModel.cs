using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using WhereToFly.Core.Services;
using WhereToFly.Logic.Model;
using Xamarin.Forms;

namespace WhereToFly.Core.ViewModels
{
    /// <summary>
    /// View model for the settings page
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly AppSettings appSettings;

        #region Binding properties
        /// <summary>
        /// List of available map overlay types
        /// </summary>
        public ObservableCollection<MapOverlayType> MapOverlayTypeItems
        {
            get; private set;
        }

        /// <summary>
        /// Currently selected map overlay type
        /// </summary>
        public MapOverlayType SelectedMapOverlayType
        {
            get
            {
                return this.appSettings.MapOverlayType;
            }
            set
            {
                this.appSettings.MapOverlayType = value;
                Task.Factory.StartNew(async () => await this.SaveSettingsAsync());
            }
        }


        /// <summary>
        /// List of available coordinate display formats
        /// </summary>
        public ObservableCollection<CoordinateDisplayFormat> CoordinateDisplayFormatItems
        {
            get; private set;
        }

        /// <summary>
        /// Currently selected coordinate display format
        /// </summary>
        public CoordinateDisplayFormat SelectedCoordinateDisplayFormatItem
        {
            get
            {
                return this.appSettings.CoordinateDisplayFormat;
            }
            set
            {
                this.appSettings.CoordinateDisplayFormat = value;
                Task.Factory.StartNew(async () => await this.SaveSettingsAsync());
            }
        }

        /// <summary>
        /// List of available map shading modes
        /// </summary>
        public ObservableCollection<MapShadingMode> MapShadingModeItems
        {
            get; private set;
        }

        /// <summary>
        /// Currently selected map shading mode
        /// </summary>
        public MapShadingMode SelectedMapShadingModeItem
        {
            get
            {
                return this.appSettings.ShadingMode;
            }
            set
            {
                this.appSettings.ShadingMode = value;
                Task.Factory.StartNew(async () => await this.SaveSettingsAsync());
            }
        }
        #endregion

        /// <summary>
        /// Creates new settings view model
        /// </summary>
        public SettingsViewModel()
        {
            this.appSettings = App.Settings;

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            this.MapOverlayTypeItems = new ObservableCollection<MapOverlayType>
            {
                MapOverlayType.OpenStreetMap,
            };

            this.CoordinateDisplayFormatItems = new ObservableCollection<CoordinateDisplayFormat>
            {
                CoordinateDisplayFormat.Format_dd_dddddd,
                CoordinateDisplayFormat.Format_dd_mm_mmm,
                CoordinateDisplayFormat.Format_dd_mm_sss,
            };

            this.MapShadingModeItems = new ObservableCollection<MapShadingMode>
            {
                MapShadingMode.Fixed10Am,
                // TODO add others
            };
        }

        /// <summary>
        /// Saves settings to data service
        /// </summary>
        private async Task SaveSettingsAsync()
        {
            var dataService = DependencyService.Get<DataService>();
            await dataService.StoreAppSettingsAsync(this.appSettings);
        }

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Event that gets signaled when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Call this method to signal that a property has changed
        /// </summary>
        /// <param name="propertyName">property name; use C# 6 nameof() operator</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
