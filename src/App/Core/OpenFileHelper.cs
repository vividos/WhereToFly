using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereToFly.App.Core.Views;
using WhereToFly.App.Geo.DataFormats;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Helper for opening files, which can either contain locations, tracks or both
    /// </summary>
    public static class OpenFileHelper
    {
        /// <summary>
        /// Opens file from given stream
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="filename">
        /// filename; extension of filename is used to determine file type
        /// </param>
        /// <returns>task to wait on</returns>
        public static async Task OpenFileAsync(Stream stream, string filename)
        {
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
        /// Opens and load location list from given stream object
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="filename">
        /// filename; extension of filename is used to determine file type
        /// </param>
        /// <returns>task to wait on</returns>
        public static async Task OpenLocationListAsync(Stream stream, string filename)
        {
            var waitingDialog = new WaitingPopupPage("Importing locations...");

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

                bool appendToList = await AskAppendToList();

                var dataService = DependencyService.Get<IDataService>();

                if (appendToList)
                {
                    var currentList = await dataService.GetLocationListAsync(CancellationToken.None);
                    locationList.InsertRange(0, currentList);
                }

                await dataService.StoreLocationListAsync(locationList);

                App.ShowToast("Locations were loaded.");

                App.UpdateMapLocationsList();
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while loading locations: " + ex.Message,
                    "OK");

                return;
            }
            finally
            {
                await waitingDialog.HideAsync();
            }
        }

        /// <summary>
        /// Opens and loads track from given stream object
        /// </summary>
        /// <param name="stream">stream object</param>
        /// <param name="filename">
        /// filename; extension of filename is used to determine file type
        /// </param>
        /// <returns>task to wait on</returns>
        public static async Task OpenTrackAsync(Stream stream, string filename)
        {
            var waitingDialog = new WaitingPopupPage("Importing track...");

            try
            {
                await waitingDialog.ShowAsync();

                var geoDataFile = GeoLoader.LoadGeoDataFile(stream, filename);

                if (!geoDataFile.GetTrackList().Any())
                {
                    await App.Current.MainPage.DisplayAlert(
                        Constants.AppTitle,
                        "No tracks were found in the file",
                        "OK");

                    return;
                }

                await SelectAndImportTrackAsync(geoDataFile);
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
                await waitingDialog.HideAsync();
            }
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
            bool appendToList = await AskAppendToList();

            var dataService = DependencyService.Get<IDataService>();

            if (appendToList)
            {
                var currentList = await dataService.GetLocationListAsync(CancellationToken.None);
                locationList.InsertRange(0, currentList);
            }

            await dataService.StoreLocationListAsync(locationList);

            App.ShowToast("Locations were loaded.");

            App.UpdateMapLocationsList();
        }

        /// <summary>
        /// Selects a single file in the track list and imports the track
        /// </summary>
        /// <param name="geoDataFile">geo data file to import from</param>
        /// <returns>task to wait on</returns>
        private static async Task SelectAndImportTrackAsync(IGeoDataFile geoDataFile)
        {
            var trackList = geoDataFile.GetTrackList();

            if (trackList.Count == 1)
            {
                await ImportTrackAsync(geoDataFile, 0);
                return;
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
                await ImportTrackAsync(geoDataFile, index);
            }
        }

        /// <summary>
        /// Imports a single track and adds it to the map
        /// </summary>
        /// <param name="geoDataFile">geo data file to import track from</param>
        /// <param name="trackIndex">track index</param>
        /// <returns>task to wait on</returns>
        private static async Task ImportTrackAsync(IGeoDataFile geoDataFile, int trackIndex)
        {
            var track = geoDataFile.LoadTrack(trackIndex);

            if (track == null)
            {
                return;
            }

            track = await AddTrackPopupPage.ShowAsync(track);
            if (track == null)
            {
                return; // user canceled editing track properties
            }

            var dataService = DependencyService.Get<IDataService>();

            var currentList = await dataService.GetTrackListAsync(CancellationToken.None);
            currentList.Add(track);

            await dataService.StoreTrackListAsync(currentList);

            App.AddMapTrack(track);

            App.ShowToast("Track was loaded.");
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
                    "Import Tracks"
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
    }
}
