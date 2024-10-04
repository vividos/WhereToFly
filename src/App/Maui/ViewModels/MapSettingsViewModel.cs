using WhereToFly.App.MapView;
using WhereToFly.App.Models;
using WhereToFly.Geo;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the map settings page
    /// </summary>
    public class MapSettingsViewModel : ViewModelBase
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

            /// <summary>
            /// Creates a new map imagery type view model
            /// </summary>
            /// <param name="text">display text</param>
            /// <param name="imageryType">imagery type</param>
            public MapImageryTypeViewModel(string text, MapImageryType imageryType)
            {
                this.Text = text;
                this.Value = imageryType;
            }
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

            /// <summary>
            /// Creates a new map overlay type view model
            /// </summary>
            /// <param name="text">display text</param>
            /// <param name="overlayType">overlay type</param>
            public MapOverlayTypeViewModel(string text, MapOverlayType overlayType)
            {
                this.Text = text;
                this.Value = overlayType;
            }
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

            /// <summary>
            /// Creates a new coordinates display format view model
            /// </summary>
            /// <param name="text">display text</param>
            /// <param name="displayFormat">coordinate display type</param>
            public CoordinateDisplayFormatViewModel(string text, CoordinateDisplayFormat displayFormat)
            {
                this.Text = text;
                this.Value = displayFormat;
            }
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

            /// <summary>
            /// Creates a new map shading mode view model
            /// </summary>
            /// <param name="text">display text</param>
            /// <param name="shadingMode">map shading mode display type</param>
            public MapShadingModeViewModel(string text, MapShadingMode shadingMode)
            {
                this.Text = text;
                this.Value = shadingMode;
            }
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
        public MapImageryTypeViewModel? SelectedMapImageryType
        {
            get
            {
                return this.MapImageryTypeItems.Find(
                    x => x.Value == this.appSettings.MapImageryType);
            }

            set
            {
                if (value?.Value != null &&
                    this.appSettings.MapImageryType != value.Value)
                {
                    this.appSettings.MapImageryType = value.Value;
                    Task.Run(this.SaveSettingsAsync);
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
        public MapOverlayTypeViewModel? SelectedMapOverlayType
        {
            get
            {
                return this.MapOverlayTypeItems.Find(
                    x => x.Value == this.appSettings.MapOverlayType);
            }

            set
            {
                if (value?.Value != null &&
                    this.appSettings.MapOverlayType != value.Value)
                {
                    this.appSettings.MapOverlayType = value.Value;
                    Task.Run(async () =>
                    {
                        await this.SaveSettingsAsync();
                        if (value.Value == MapOverlayType.OpenFlightMaps)
                        {
                            var appMapService = DependencyService.Get<IAppMapService>();
                            await appMapService.ShowFlightPlanningDisclaimer();
                        }
                    });
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
        public CoordinateDisplayFormatViewModel? SelectedCoordinateDisplayFormatItem
        {
            get
            {
                return this.CoordinateDisplayFormatItems.Find(
                    x => x.Value == this.appSettings.CoordinateDisplayFormat);
            }

            set
            {
                if (value?.Value != null &&
                    this.appSettings.CoordinateDisplayFormat != value.Value)
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
        public MapShadingModeViewModel? SelectedMapShadingModeItem
        {
            get
            {
                return this.MapShadingModeItems.Find(
                    x => x.Value == this.appSettings.ShadingMode);
            }

            set
            {
                if (value?.Value != null &&
                    this.appSettings.ShadingMode != value.Value)
                {
                    this.appSettings.ShadingMode = value.Value;
                    Task.Run(async () => await this.SaveSettingsAsync());
                }
            }
        }

        /// <summary>
        /// Current value of the "entity clustering" switch state
        /// </summary>
        public bool MapEntityClustering
        {
            get
            {
                return this.appSettings.UseMapEntityClustering;
            }

            set
            {
                if (this.appSettings.UseMapEntityClustering != value)
                {
                    this.appSettings.UseMapEntityClustering = value;
                    Task.Run(async () => await this.SaveSettingsAsync());
                }
            }
        }
        #endregion

        /// <summary>
        /// Creates new map settings view model
        /// </summary>
        public MapSettingsViewModel()
        {
            this.appSettings = App.Settings!;

            this.MapImageryTypeItems =
            [
                new MapImageryTypeViewModel("OpenStreetMap", MapImageryType.OpenStreetMap),
                new MapImageryTypeViewModel("Aerials + Labels (Bing Maps)", MapImageryType.BingMapsAerialWithLabels),
                new MapImageryTypeViewModel("Sentinel-2 cloudless", MapImageryType.Sentinel2),
                new MapImageryTypeViewModel("OpenTopoMap", MapImageryType.OpenTopoMap),
            ];

            this.MapOverlayTypeItems =
            [
                new MapOverlayTypeViewModel("None", MapOverlayType.None),
                new MapOverlayTypeViewModel("Thermal Skyways (thermal.kk7.ch)", MapOverlayType.ThermalSkywaysKk7),
                new MapOverlayTypeViewModel("Contour lines", MapOverlayType.ContourLines),
                new MapOverlayTypeViewModel("Slope + contour lines", MapOverlayType.SlopeAndContourLines),
                new MapOverlayTypeViewModel("NASA Black Marble 2017", MapOverlayType.BlackMarble),
                new MapOverlayTypeViewModel("Waymarked Trails Hiking", MapOverlayType.WaymarkedTrailsHiking),
                new MapOverlayTypeViewModel("OpenFlightMaps", MapOverlayType.OpenFlightMaps),
            ];

            this.CoordinateDisplayFormatItems =
            [
                new CoordinateDisplayFormatViewModel("dd.dddddd°", CoordinateDisplayFormat.Format_dd_dddddd),
                new CoordinateDisplayFormatViewModel("dd° mm.mmm'", CoordinateDisplayFormat.Format_dd_mm_mmm),
                new CoordinateDisplayFormatViewModel("dd° mm' sss\"", CoordinateDisplayFormat.Format_dd_mm_sss),
            ];

            this.MapShadingModeItems =
            [
                new MapShadingModeViewModel("Fixed at 10 a.m.", MapShadingMode.Fixed10Am),
                new MapShadingModeViewModel("Fixed at 3 p.m.", MapShadingMode.Fixed3Pm),
                new MapShadingModeViewModel("Follow current time", MapShadingMode.CurrentTime),
                new MapShadingModeViewModel("Current time + 6 hours", MapShadingMode.Ahead6Hours),
                new MapShadingModeViewModel("No shading", MapShadingMode.None),
            ];
        }

        /// <summary>
        /// Saves settings to data service
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task SaveSettingsAsync()
        {
            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(this.appSettings);

            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.UpdateMapSettings();

            UserInterface.DisplayToast("Settings were saved.");
        }
    }
}
