using System.Collections.Generic;
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
        /// <summary>
        /// View model for map overlay type
        /// </summary>
        public class MapOverlayTypeViewModel
        {
            /// <summary>
            /// Display text for value
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Map overlay type value
            /// </summary>
            public MapOverlayType Value { get; set; }
        }

        /// <summary>
        /// View model for coordinate display format
        /// </summary>
        public class CoordinateDisplayFormatViewModel
        {
            /// <summary>
            /// Display text for value
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Coordinate display format value
            /// </summary>
            public CoordinateDisplayFormat Value { get; set; }
        }

        /// <summary>
        /// View model for map shading mode
        /// </summary>
        public class MapShadingModeViewModel
        {
            /// <summary>
            /// Display text for value
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Map shading mode value
            /// </summary>
            public MapShadingMode Value { get; set; }
        }

        /// <summary>
        /// App settings object
        /// </summary>
        private readonly AppSettings appSettings;

        #region Binding properties
        /// <summary>
        /// List of available map overlay types
        /// </summary>
        public List<MapOverlayTypeViewModel> MapOverlayTypeItems
        {
            get; private set;
        }

        /// <summary>
        /// Currently selected map overlay type
        /// </summary>
        public MapOverlayTypeViewModel SelectedMapOverlayType
        {
            get
            {
                return this.MapOverlayTypeItems.Find(x => x.Value == this.appSettings.MapOverlayType);
            }

            set
            {
                this.appSettings.MapOverlayType = value.Value;
                Task.Factory.StartNew(async () => await this.SaveSettingsAsync());
            }
        }

        /// <summary>
        /// List of available coordinate display formats
        /// </summary>
        public List<CoordinateDisplayFormatViewModel> CoordinateDisplayFormatItems
        {
            get; private set;
        }

        /// <summary>
        /// Currently selected coordinate display format
        /// </summary>
        public CoordinateDisplayFormatViewModel SelectedCoordinateDisplayFormatItem
        {
            get
            {
                return this.CoordinateDisplayFormatItems.Find(x => x.Value == this.appSettings.CoordinateDisplayFormat);
            }

            set
            {
                this.appSettings.CoordinateDisplayFormat = value.Value;
                Task.Factory.StartNew(async () => await this.SaveSettingsAsync());
            }
        }

        /// <summary>
        /// List of available map shading modes
        /// </summary>
        public List<MapShadingModeViewModel> MapShadingModeItems
        {
            get; private set;
        }

        /// <summary>
        /// Currently selected map shading mode
        /// </summary>
        public MapShadingModeViewModel SelectedMapShadingModeItem
        {
            get
            {
                return this.MapShadingModeItems.Find(x => x.Value == this.appSettings.ShadingMode);
            }

            set
            {
                this.appSettings.ShadingMode = value.Value;
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
            this.MapOverlayTypeItems = new List<MapOverlayTypeViewModel>
            {
                new MapOverlayTypeViewModel { Text = "OpenStreetMap", Value = MapOverlayType.OpenStreetMap },
            };

            this.CoordinateDisplayFormatItems = new List<CoordinateDisplayFormatViewModel>
            {
                new CoordinateDisplayFormatViewModel { Text = "dd.dddddd°", Value = CoordinateDisplayFormat.Format_dd_dddddd },
                new CoordinateDisplayFormatViewModel { Text = "dd° mm.mmm'", Value = CoordinateDisplayFormat.Format_dd_mm_mmm },
                new CoordinateDisplayFormatViewModel { Text = "dd° mm' sss\"", Value = CoordinateDisplayFormat.Format_dd_mm_sss },
            };

            this.MapShadingModeItems = new List<MapShadingModeViewModel>
            {
                new MapShadingModeViewModel { Text = "Fixed at 10 a.m.", Value = MapShadingMode.Fixed10Am },
                new MapShadingModeViewModel { Text = "Fixed at 3 p.m.", Value = MapShadingMode.Fixed3Pm },
                new MapShadingModeViewModel { Text = "Follow current time", Value = MapShadingMode.CurrentTime },
                new MapShadingModeViewModel { Text = "Current time + 2 hours", Value = MapShadingMode.Ahead2Hours },
            };
        }

        /// <summary>
        /// Saves settings to data service
        /// </summary>
        /// <returns>task to wait on</returns>
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
