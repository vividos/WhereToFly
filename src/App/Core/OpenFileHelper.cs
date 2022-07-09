using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Logic;
using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.Views;
using WhereToFly.Geo;
using WhereToFly.Geo.Airspace;
using WhereToFly.Geo.DataFormats;
using WhereToFly.Geo.DataFormats.Czml;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Helper for opening files, which can either contain locations, tracks or both
    /// </summary>
    public static class OpenFileHelper
    {
        /// <summary>
        /// Waiting dialog that is currently shown; else it's set to null
        /// </summary>
        private static WaitingPopupPage waitingDialog;

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

                bool hasTracks = geoDataFile.GetTrackList().Any();

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
                    await App.Current.MainPage.DisplayAlert(
                        Constants.AppTitle,
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

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
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

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
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
                await waitingDialog.ShowAsync();

                var geoDataFile = GeoLoader.LoadGeoDataFile(stream, filename);
                var locationList = geoDataFile.HasLocations() ? geoDataFile.LoadLocationList() : null;

                if (locationList == null ||
                    !locationList.Any())
                {
                    await App.Current.MainPage.DisplayAlert(
                        Constants.AppTitle,
                        "No locations were found in the file",
                        "OK");

                    return;
                }

                await ImportLocationListAsync(locationList);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
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
                await waitingDialog.ShowAsync();

                var geoDataFile = GeoLoader.LoadGeoDataFile(stream, filename);

                bool hasTracks = geoDataFile.GetTrackList().Any();
                if (!hasTracks)
                {
                    await App.Current.MainPage.DisplayAlert(
                        Constants.AppTitle,
                        "No tracks were found in the file",
                        "OK");

                    return false;
                }

                return await SelectAndImportTrackAsync(geoDataFile);
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
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
            bool appendToList = await App.Current.MainPage.DisplayAlert(
                Constants.AppTitle,
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

            var dataService = DependencyService.Get<IDataService>();
            var locationDataService = dataService.GetLocationDataService();

            bool isListEmpty = await locationDataService.IsListEmpty();
            if (!isListEmpty)
            {
                bool appendToList = await AskAppendToList();

                if (!appendToList)
                {
                    await locationDataService.ClearList();
                    App.MapView.ClearLocationList();
                }
            }

            await locationDataService.AddList(locationList);

            var liveWaypointRefreshService = DependencyService.Get<LiveDataRefreshService>();
            liveWaypointRefreshService.ClearLiveWaypointList();
            liveWaypointRefreshService.AddLiveWaypointList(locationList);

            await NavigationService.GoToMap();

            App.ShowToast("Locations were loaded.");

            App.MapView.AddLocationList(locationList);

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
                if (TakeoffDirectionsHelper.TryParse(location.Name, out TakeoffDirections takeoffDirections))
                {
                    location.TakeoffDirections = takeoffDirections;
                }
                else if (TryParseStartDirectionFromDescription(location.Description, out TakeoffDirections takeoffDirections2))
                {
                    location.TakeoffDirections = takeoffDirections2;
                }
            }
        }

        /// <summary>
        /// Tries to parse a start direction from given location description. Some locations, e.g.
        /// from the DHV Geländedatenbank, have the start directions in the description, after a
        /// certain text.
        /// </summary>
        /// <param name="description">location description</param>
        /// <param name="takeoffDirections">parsed takeoff directions</param>
        /// <returns>true when a takeoff direction could be parsed, or false when not</returns>
        private static bool TryParseStartDirectionFromDescription(string description, out TakeoffDirections takeoffDirections)
        {
            takeoffDirections = TakeoffDirections.None;

            const string StartDirectionText = "Startrichtung ";
            int posStartDirection = description.IndexOf(StartDirectionText);
            if (posStartDirection == -1)
            {
                return false;
            }

            int posLineBreak = description.IndexOf("<br", posStartDirection);
            if (posLineBreak == -1)
            {
                posLineBreak = description.Length;
            }

            posStartDirection += StartDirectionText.Length;

            string direction = description.Substring(posStartDirection, posLineBreak - posStartDirection);

            return TakeoffDirectionsHelper.TryParse(direction, out takeoffDirections);
        }

        /// <summary>
        /// Zooms to location list
        /// </summary>
        /// <param name="locationList">list of all locations</param>
        private static void ZoomToLocationList(List<Location> locationList)
        {
            locationList.GetBoundingRectangle(out MapPoint minLocation, out MapPoint maxLocation);

            App.MapView.ZoomToRectangle(minLocation, maxLocation);
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
            foreach (var trackName in trackList)
            {
                count++;
                choices.Add($"{count}. {trackName}");
            }

            string choice = await App.Current.MainPage.DisplayActionSheet(
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

            track = await AddTrackPopupPage.ShowAsync(track);
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

            await App.AddTrack(track);
            App.MapView.ZoomToTrack(track);

            App.ShowToast("Track was loaded.");

            return true;
        }

        /// <summary>
        /// Samples track point heights and adjusts the track when it goes below terrain height.
        /// </summary>
        /// <param name="track">track to adjust point heights</param>
        /// <returns>task to wait on</returns>
        private static async Task AdjustTrackHeightsAsync(Track track)
        {
            var cts = new CancellationTokenSource();

            waitingDialog = new WaitingPopupPage("Sampling track point heights...", cts);
            await App.RunOnUiThreadAsync(async () => await waitingDialog.ShowAsync());

            await App.InitializedTask;
            try
            {
                var trackPointHeights =
                    await Task.Run(
                        async () => await App.MapView.SampleTrackHeights(track),
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

            var choices = new string[2]
                {
                    "Import Locations",
                    "Import Tracks",
                };

            string choice = await App.Current.MainPage.DisplayActionSheet(
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
                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
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
                czml = streamReader.ReadToEnd();
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

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
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
            var airspaceClasses = airspacesList.Select(airspace => airspace.Class).Distinct();

            var selectedAirspaceClasses = await SelectAirspaceClassPopupPage.ShowAsync(airspaceClasses);
            if (selectedAirspaceClasses == null)
            {
                return Enumerable.Empty<Airspace>();
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
                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "The file contained no valid CZML layer data",
                    "OK");

                return;
            }

            ReadCzmlNameAndDescription(czml, out string name, out string description);

            if (string.IsNullOrEmpty(name))
            {
                name = Path.GetFileNameWithoutExtension(filename);
            }

            var layer = new Layer
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = name,
                Description = HtmlConverter.Sanitize(description + fileDescription),
                IsVisible = true,
                LayerType = LayerType.CzmlLayer,
                Data = czml,
            };

            layer = await NavigationService.Instance.NavigateToPopupPageAsync<Layer>(
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

            await App.ShowFlightPlanningDisclaimerAsync();

            await NavigationService.GoToMap();

            App.MapView.AddLayer(layer);
            App.MapView.ZoomToLayer(layer);

            App.ShowToast("Layer was loaded.");
        }

        /// <summary>
        /// Reads name and description fields from the CZML file's packet header, if present.
        /// </summary>
        /// <param name="czml">CZML JSON text</param>
        /// <param name="name">document name to read</param>
        /// <param name="description">document description to read</param>
        private static void ReadCzmlNameAndDescription(string czml, out string name, out string description)
        {
            var rootArray = JArray.Parse(czml);
            if (rootArray.Count > 0)
            {
                var headerObject = rootArray[0].ToObject<PacketHeader>();
                if (headerObject != null &&
                    headerObject.Id == "document")
                {
                    name = headerObject.Name;
                    description = headerObject.Description;

                    return;
                }
            }

            name = string.Empty;
            description = string.Empty;
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
            if ((!json.StartsWith("{") || !json.EndsWith("}")) &&
                (!json.StartsWith("[") || !json.EndsWith("]")))
            {
                return false;
            }

            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException jex)
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
