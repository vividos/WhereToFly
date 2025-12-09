using System.Text.Json;
using WhereToFly.App.Abstractions;
using WhereToFly.App.Popups;
using WhereToFly.App.Services;
using WhereToFly.Geo;
using WhereToFly.Geo.Airspace;
using WhereToFly.Geo.DataFormats;
using WhereToFly.Geo.Model;
using Location = WhereToFly.Geo.Model.Location;

namespace WhereToFly.App.Logic
{
    /// <summary>
    /// Helper for opening files, which can either contain locations, tracks or both
    /// </summary>
    public static class OpenFileHelper
    {
        /// <summary>
        /// Access to the user interface
        /// </summary>
        private static IUserInterface UserInterface
            => DependencyService.Get<IUserInterface>();

        /// <summary>
        /// Waiting dialog that is currently shown; else it's set to null
        /// </summary>
        private static WaitingPopupPage? waitingDialog;

        /// <summary>
        /// Opens file from given stream.
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="filename">
        /// filename; extension of filename is used to determine file type
        /// </param>
        /// <returns>task to wait on</returns>
        public static async Task OpenFileAsync(Stream stream, string filename)
        {
            if (Path.GetExtension(filename).ToLowerInvariant() == ".czml")
            {
                await ImportLayerFile(stream, filename);
                return;
            }

            if (Path.GetExtension(filename).ToLowerInvariant() == ".txt")
            {
                await ImportOpenAirAirspaceFile(stream, filename);
                return;
            }

            try
            {
                var geoDataFile = GeoLoader.LoadGeoDataFile(stream, filename);

                bool hasTracks = geoDataFile.GetTrackList().Count != 0;

                if (geoDataFile.HasLocations() && !hasTracks)
                {
                    await ImportLocationListAsync(geoDataFile.LoadLocationList());
                    return;
                }

                if (!geoDataFile.HasLocations() && hasTracks)
                {
                    await SelectAndImportTrackAsync(geoDataFile);
                    return;
                }

                if (!geoDataFile.HasLocations() && !hasTracks)
                {
                    await UserInterface.DisplayAlert(
                        "No locations or tracks were found in the file",
                        "OK");

                    return;
                }

                // file has both locations and tracks; ask user
                await AskUserImportLocationsOrTracks(geoDataFile);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await UserInterface.DisplayAlert(
                    "Error while opening file: " + ex.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Adds (or replaces) the default location list
        /// </summary>
        /// <returns>task to wait on</returns>
        public static async Task AddDefaultLocationListAsync()
        {
            try
            {
                var locationList = DataServiceHelper.GetDefaultLocationList();

                await ImportLocationListAsync(locationList);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await UserInterface.DisplayAlert(
                    "Error while loading default locations: " + ex.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Opens and load location list from given stream object
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="filename">
        /// filename; extension of filename is used to determine file type
        /// </param>
        /// <returns>task to wait on</returns>
        public static async Task OpenLocationListAsync(Stream stream, string filename)
        {
            await CloseWaitingPopupPageAsync();
            waitingDialog = new WaitingPopupPage("Importing locations...");

            try
            {
                waitingDialog.Show();

                var geoDataFile = GeoLoader.LoadGeoDataFile(stream, filename);
                var locationList = geoDataFile.HasLocations() ? geoDataFile.LoadLocationList() : null;

                if (locationList == null ||
                    locationList.Count == 0)
                {
                    await UserInterface.DisplayAlert(
                        "No locations were found in the file",
                        "OK");

                    return;
                }

                await ImportLocationListAsync(locationList);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await UserInterface.DisplayAlert(
                    "Error while loading locations: " + ex.Message,
                    "OK");
            }
            finally
            {
                await CloseWaitingPopupPageAsync();
            }
        }

        /// <summary>
        /// Opens and loads track from given stream object
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="filename">
        /// filename; extension of filename is used to determine file type
        /// </param>
        /// <returns>true when loading track was successful, false when not</returns>
        public static async Task<bool> OpenTrackAsync(Stream stream, string filename)
        {
            waitingDialog = new WaitingPopupPage("Importing track...");

            try
            {
                waitingDialog.Show();

                var geoDataFile = GeoLoader.LoadGeoDataFile(stream, filename);

                bool hasTracks = geoDataFile.GetTrackList().Count != 0;
                if (!hasTracks)
                {
                    await UserInterface.DisplayAlert(
                        "No tracks were found in the file",
                        "OK");

                    return false;
                }

                return await SelectAndImportTrackAsync(geoDataFile);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await UserInterface.DisplayAlert(
                    $"Error while loading track: {ex.Message}",
                    "OK");
            }
            finally
            {
                await CloseWaitingPopupPageAsync();
            }

            return false;
        }

        /// <summary>
        /// Asks user if location list should be appended or replaced
        /// </summary>
        /// <returns>true when location list should be appended, false when it should be
        /// replaced</returns>
        private static async Task<bool> AskAppendToList()
        {
            bool appendToList = await UserInterface.DisplayAlert(
                "Append to current location list or replace list?",
                "Append",
                "Replace");

            return appendToList;
        }

        /// <summary>
        /// Imports loaded location list by appending or replacing the location list present.
        /// </summary>
        /// <param name="locationList">location list to import</param>
        /// <returns>task to wait on</returns>
        private static async Task ImportLocationListAsync(List<Location> locationList)
        {
            SanitizeLocationDescriptions(locationList);
            AddTakeoffDirections(locationList);

            var appMapService = DependencyService.Get<IAppMapService>();

            var dataService = DependencyService.Get<IDataService>();
            var locationDataService = dataService.GetLocationDataService();

            bool isListEmpty = await locationDataService.IsListEmpty();
            if (!isListEmpty)
            {
                bool appendToList = await AskAppendToList();

                if (!appendToList)
                {
                    await locationDataService.ClearList();
                    appMapService.MapView.ClearLocationList();
                }
            }

            await locationDataService.AddList(locationList);

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.ClearLiveWaypointList();
            liveWaypointRefreshService.AddLiveWaypointList(locationList);

            await UserInterface.NavigationService.GoToMap();

            UserInterface.DisplayToast("Locations were loaded.");

            await appMapService.MapView.AddLocationList(locationList);

            ZoomToLocationList(locationList);
        }

        /// <summary>
        /// Sanitizes all descriptions of all locations in the list
        /// </summary>
        /// <param name="locationList">location list</param>
        private static void SanitizeLocationDescriptions(List<Location> locationList)
        {
            foreach (var location in locationList)
            {
                location.Description = HtmlConverter.Sanitize(location.Description);
            }
        }

        /// <summary>
        /// Adds takeoff directions value to every location in the list
        /// </summary>
        /// <param name="locationList">location list to modify</param>
        private static void AddTakeoffDirections(List<Location> locationList)
        {
            foreach (var location in locationList)
            {
                TakeoffDirectionsHelper.AddTakeoffDirection(location);
            }
        }

        /// <summary>
        /// Zooms to location list
        /// </summary>
        /// <param name="locationList">list of all locations</param>
        private static void ZoomToLocationList(List<Location> locationList)
        {
            locationList.GetBoundingRectangle(out MapPoint minLocation, out MapPoint maxLocation);

            var appMapService = DependencyService.Get<IAppMapService>();
            appMapService.MapView.ZoomToRectangle(minLocation, maxLocation);
        }

        /// <summary>
        /// Selects a single file in the track list and imports the track
        /// </summary>
        /// <param name="geoDataFile">geo data file to import from</param>
        /// <returns>true when loading track was successful, false when not</returns>
        private static async Task<bool> SelectAndImportTrackAsync(IGeoDataFile geoDataFile)
        {
            var trackList = geoDataFile.GetTrackList();

            if (trackList.Count == 1)
            {
                return await ImportTrackAsync(geoDataFile, 0);
            }

            string question = "Select track to import";

            var choices = new List<string>();
            int count = 0;
            foreach (string trackName in trackList)
            {
                count++;
                choices.Add($"{count}. {trackName}");
            }

            string choice = await UserInterface.DisplayActionSheet(
                question,
                "Cancel",
                null,
                choices.ToArray());

            int index = choices.IndexOf(choice);
            if (index != -1)
            {
                return await ImportTrackAsync(geoDataFile, index);
            }

            return false;
        }

        /// <summary>
        /// Imports a single track and adds it to the map
        /// </summary>
        /// <param name="geoDataFile">geo data file to import track from</param>
        /// <param name="trackIndex">track index</param>
        /// <returns>true when loading track was successful, false when not</returns>
        private static async Task<bool> ImportTrackAsync(IGeoDataFile geoDataFile, int trackIndex)
        {
            var track = geoDataFile.LoadTrack(trackIndex);

            if (track == null)
            {
                return false;
            }

            track.Description = HtmlConverter.Sanitize(track.Description);

            track.CalculateStatistics();

            await CloseWaitingPopupPageAsync();

            track = await UserInterface.NavigationService.NavigateToPopupPageAsync<Track?>(
                PopupPageKey.AddTrackPopupPage,
                true,
                track);

            if (track == null)
            {
                return false; // user canceled editing track properties
            }

            // sample track heights only when it's a flight; non-flight tracks are draped onto
            // the ground
            if (track.IsFlightTrack)
            {
                await AdjustTrackHeightsAsync(track);
            }

            var dataService = DependencyService.Get<IDataService>();
            var trackDataService = dataService.GetTrackDataService();

            await trackDataService.Add(track);

            if (track.IsLiveTrack)
            {
                var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
                liveWaypointRefreshService.AddLiveTrack(track);
            }

            var appMapService = DependencyService.Get<IAppMapService>();
            await appMapService.AddTrack(track);
            appMapService.MapView.ZoomToTrack(track);

            UserInterface.DisplayToast("Track was loaded.");

            return true;
        }

        /// <summary>
        /// Samples track point heights and adjusts the track when it goes below terrain height.
        /// </summary>
        /// <param name="track">track to adjust point heights</param>
        /// <returns>task to wait on</returns>
        internal static async Task AdjustTrackHeightsAsync(Track track)
        {
            var cts = new CancellationTokenSource();

            waitingDialog = new WaitingPopupPage("Sampling track point heights...", cts);

            try
            {
                await MainThread.InvokeOnMainThreadAsync(
                    () => waitingDialog.Show());

                await App.InitializedTask;

                var appMapService = DependencyService.Get<IAppMapService>();

                double[]? trackPointHeights =
                    await Task.Run(
                        async () => await appMapService.MapView.SampleTrackHeights(track),
                        cts.Token);

                if (cts.IsCancellationRequested)
                {
                    return;
                }

                if (trackPointHeights != null)
                {
                    track.GroundHeightProfile = trackPointHeights.ToList();
                    track.AdjustTrackPointsByGroundProfile(trackPointHeights);
                }
            }
            finally
            {
                await waitingDialog.HideAsync();
                waitingDialog = null;
            }
        }

        /// <summary>
        /// Asks user if locations or tracks should be imported
        /// </summary>
        /// <param name="geoDataFile">geo data file to import from</param>
        /// <returns>task to wait on</returns>
        private static async Task AskUserImportLocationsOrTracks(IGeoDataFile geoDataFile)
        {
            string question = "What do you want to import?";

            string[] choices =
            [
                "Import Locations",
                "Import Tracks",
            ];

            string choice = await UserInterface.DisplayActionSheet(
                question,
                null,
                null,
                choices[0],
                choices[1]);

            if (choice == choices[0])
            {
                await ImportLocationListAsync(geoDataFile.LoadLocationList());
            }
            else if (choice == choices[1])
            {
                await SelectAndImportTrackAsync(geoDataFile);
            }
        }

        /// <summary>
        /// Checks for file extension and then imports layer file
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <param name="filename">filename of file</param>
        /// <returns>task to wait on</returns>
        public static async Task OpenLayerFileAsync(Stream stream, string filename)
        {
            if (Path.GetExtension(filename).ToLowerInvariant() != ".czml")
            {
                await UserInterface.DisplayAlert(
                    "The file is not a CZML layer data file",
                    "OK");

                return;
            }

            await ImportLayerFile(stream, filename);
        }

        /// <summary>
        /// Imports a .czml layer file
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <param name="filename">filename of file</param>
        /// <returns>task to wait on</returns>
        private static async Task ImportLayerFile(Stream stream, string filename)
        {
            string czml;
            using (var streamReader = new StreamReader(stream))
            {
                czml = await streamReader.ReadToEndAsync();
            }

            await AddLayerFromCzml(czml, filename, string.Empty);
        }

        /// <summary>
        /// Imports a .txt OpenAir airspace file and adds it as layer
        /// </summary>
        /// <param name="stream">stream of file to import</param>
        /// <param name="filename">filename of file to import</param>
        /// <returns>task to wait on</returns>
        public static async Task ImportOpenAirAirspaceFile(Stream stream, string filename)
        {
            try
            {
                var parser = new OpenAirFileParser(stream);

                var filteredAirspaces = await SelectAirspaceClassesToImport(parser.Airspaces);
                if (!filteredAirspaces.Any())
                {
                    return;
                }

                string czml = CzmlAirspaceWriter.WriteCzml(
                    Path.GetFileNameWithoutExtension(filename),
                    filteredAirspaces,
                    parser.FileCommentLines);

                string fileComments = string.Join("\n", parser.FileCommentLines);

                if (parser.ParsingErrors.Any())
                {
                    fileComments += "\nParsing errors:\n";
                    fileComments +=
                        string.Join("\n", parser.ParsingErrors);
                }

                string description =
                    fileComments
                    .Replace("\n\n", "\n")
                    .Trim();

                await AddLayerFromCzml(czml, filename, description);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await UserInterface.DisplayAlert(
                    $"Error while loading OpenAir airspaces: {ex.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// Filters airspaces list by letting the user select all airspace classes that should be
        /// imported.
        /// </summary>
        /// <param name="airspacesList">list of all airspaces</param>
        /// <returns>filtered list of all airspaces</returns>
        private static async Task<IEnumerable<Airspace>> SelectAirspaceClassesToImport(IEnumerable<Airspace> airspacesList)
        {
            var airspaceClasses = airspacesList
                .Select(airspace => airspace.Class)
                .Distinct()
                .ToList();

            var selectedAirspaceClasses = await UserInterface.NavigationService.NavigateToPopupPageAsync<ISet<AirspaceClass>>(
                PopupPageKey.SelectAirspaceClassPopupPage,
                true,
                airspaceClasses);

            if (selectedAirspaceClasses == null)
            {
                return [];
            }

            var filteredAirspaces = airspacesList.Where(
                airspace => selectedAirspaceClasses.Contains(airspace.Class));

            return filteredAirspaces;
        }

        /// <summary>
        /// Adds a new layer from given CZML text
        /// </summary>
        /// <param name="czml">loaded CZML text</param>
        /// <param name="filename">filename of loaded file</param>
        /// <param name="fileDescription">layer file's description</param>
        /// <returns>task to wait on</returns>
        private static async Task AddLayerFromCzml(string czml, string filename, string fileDescription)
        {
            if (!IsValidJson(czml))
            {
                await UserInterface.DisplayAlert(
                    "The file contained no valid CZML layer data",
                    "OK");

                return;
            }

            WhereToFly.Geo.DataFormats.Czml.Serializer.ReadCzmlNameAndDescription(
                czml,
                out string name,
                out string description);

            if (string.IsNullOrEmpty(name))
            {
                name = Path.GetFileNameWithoutExtension(filename);
            }

            var layer = new Layer(Guid.NewGuid().ToString("B"))
            {
                Name = name,
                Description = HtmlConverter.Sanitize(description + fileDescription),
                IsVisible = true,
                LayerType = LayerType.CzmlLayer,
                Data = czml,
            };

            layer = await UserInterface.NavigationService.NavigateToPopupPageAsync<Layer>(
                PopupPageKey.AddLayerPopupPage,
                true,
                layer);

            if (layer == null)
            {
                return;
            }

            var dataService = DependencyService.Get<IDataService>();
            var layerDataService = dataService.GetLayerDataService();

            await layerDataService.Add(layer);

            var appMapService = DependencyService.Get<IAppMapService>();
            await appMapService.ShowFlightPlanningDisclaimer();

            await UserInterface.NavigationService.GoToMap();

            await appMapService.MapView.AddLayer(layer);
            appMapService.MapView.ZoomToLayer(layer);

            UserInterface.DisplayToast("Layer was loaded.");
        }

        /// <summary>
        /// Checks if given JSON string is actually valid JSON.
        /// </summary>
        /// <param name="json">JSON string to check</param>
        /// <returns>true when valid JSON, false when not</returns>
        private static bool IsValidJson(string json)
        {
            json = json.Trim();

            // check for object or array syntax
            if ((!json.StartsWith('{') || !json.EndsWith('}')) &&
                (!json.StartsWith('[') || !json.EndsWith(']')))
            {
                return false;
            }

            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException jex)
            {
                App.LogError(jex);
                return false;
            }
            catch (Exception ex)
            {
                App.LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Closes waiting popup page when on stack (actually removes any popup page).
        /// </summary>
        /// <returns>task to wait on</returns>
        private static async Task CloseWaitingPopupPageAsync()
        {
            try
            {
                if (waitingDialog != null)
                {
                    await waitingDialog.HideAsync();
                    waitingDialog = null;
                }
            }
            catch (Exception)
            {
                // ignore when there's no popup page on stack
            }
        }
    }
}
