using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the settings page
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// View model for map imagery type
        /// </summary>
        public class MapImageryTypeViewModel
        {
            /// <summary>
            /// Display text for value
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Map imagery type value
            /// </summary>
            public MapImageryType Value { get; set; }
        }

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
        /// List of available map imagery types
        /// </summary>
        public List<MapImageryTypeViewModel> MapImageryTypeItems
        {
            get; private set;
        }

        /// <summary>
        /// Currently selected map imagery type
        /// </summary>
        public MapImageryTypeViewModel SelectedMapImageryType
        {
            get
            {
                return this.MapImageryTypeItems.Find(x => x.Value == this.appSettings.MapImageryType);
            }

            set
            {
                if (this.appSettings.MapImageryType != value.Value)
                {
                    this.appSettings.MapImageryType = value.Value;
                    Task.Run(async () => await this.SaveSettingsAsync());
                }
            }
        }

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
                if (this.appSettings.MapOverlayType != value.Value)
                {
                    this.appSettings.MapOverlayType = value.Value;
                    Task.Run(async () => await this.SaveSettingsAsync());
                }
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
                if (this.appSettings.CoordinateDisplayFormat != value.Value)
                {
                    this.appSettings.CoordinateDisplayFormat = value.Value;
                    Task.Run(async () => await this.SaveSettingsAsync());
                }
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
                if (this.appSettings.ShadingMode != value.Value)
                {
                    this.appSettings.ShadingMode = value.Value;
                    Task.Run(async () => await this.SaveSettingsAsync());
                }
            }
        }

        /// <summary>
        /// Command to clear web view cache
        /// </summary>
        public Command ClearWebViewCacheCommand { get; set; }
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
            this.MapImageryTypeItems = new List<MapImageryTypeViewModel>
            {
                new MapImageryTypeViewModel { Text = "OpenStreetMap", Value = MapImageryType.OpenStreetMap },
                new MapImageryTypeViewModel { Text = "Aerials + Labels (Bing Maps)", Value = MapImageryType.BingMapsAerialWithLabels },
                new MapImageryTypeViewModel { Text = "OpenTopoMap", Value = MapImageryType.OpenTopoMap },
            };

            this.MapOverlayTypeItems = new List<MapOverlayTypeViewModel>
            {
                new MapOverlayTypeViewModel { Text = "None", Value = MapOverlayType.None },
                new MapOverlayTypeViewModel { Text = "Thermal Skyways (thermal.kk7.ch)", Value = MapOverlayType.ThermalSkywaysKk7 },
                new MapOverlayTypeViewModel { Text = "Contour lines", Value = MapOverlayType.ContourLines },
                new MapOverlayTypeViewModel { Text = "Slope + contour lines", Value = MapOverlayType.SlopeAndContourLines },
                new MapOverlayTypeViewModel { Text = "NASA Black Marble", Value = MapOverlayType.BlackMarble },
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
                new MapShadingModeViewModel { Text = "Current time + 6 hours", Value = MapShadingMode.Ahead6Hours },
                new MapShadingModeViewModel { Text = "No shading", Value = MapShadingMode.None },
            };

            this.ClearWebViewCacheCommand = new Command(App.ClearWebViewCache);
        }

        /// <summary>
        /// Saves settings to data service
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task SaveSettingsAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(this.appSettings);

            App.UpdateMapSettings();
        }
    }
}
