using System;
using System.IO;
using System.Threading.Tasks;
using WhereToFly.App.Geo;
using WhereToFly.App.Geo.DataFormats;
using WhereToFly.Shared.Model;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Helper for exporting files
    /// </summary>
    public static class ExportFileHelper
    {
        /// <summary>
        /// Exports layer by asking user for destination path and then saving binary KML data of
        /// layer to file.
        /// </summary>
        /// <param name="layer">layer to export</param>
        /// <returns>task to wait on</returns>
        public static async Task ExportLayerAsync(Layer layer)
        {
            byte[] data = await App.MapView.ExportLayerAsync(layer);
            if (data == null)
            {
                App.ShowToast("Error occured at layer export.");
                return;
            }

            string exportFilename = await AskUserExportFilenameAsync(layer.Name + ".kmz");
            if (exportFilename == null)
            {
                return;
            }

            try
            {
                File.WriteAllBytes(exportFilename, data);

                App.ShowToast("Layer was successfully exported.");
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while exporting layer: " + ex.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Exports track by asking user for destination path and then using GpxWriter to write
        /// track to file
        /// </summary>
        /// <param name="track">track to export</param>
        /// <returns>task to wait on</returns>
        public static async Task ExportTrackAsync(Track track)
        {
            string exportFilename = await AskUserExportFilenameAsync(track.Name + ".gpx");
            if (exportFilename == null)
            {
                return;
            }

            try
            {
                GpxWriter.WriteTrack(exportFilename, track);

                App.ShowToast("Track was successfully exported.");
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Error while exporting track: " + ex.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Ask user about the export filename and return it
        /// </summary>
        /// <param name="filename">filename and extension of file to export</param>
        /// <returns>
        /// export filename, or null when editing was cancelled or an invalid path was specified
        /// </returns>
        private static async Task<string> AskUserExportFilenameAsync(string filename)
        {
            var platform = DependencyService.Get<IPlatform>();
            string exportFilename = Path.Combine(platform.PublicExportFolder, filename);

            string editedExportFilename = await App.Current.MainPage.DisplayPromptAsync(
                Constants.AppTitle,
                "Export filename",
                "Export",
                "Cancel",
                initialValue: exportFilename);

            if (string.IsNullOrEmpty(editedExportFilename))
            {
                return null;
            }

            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                return null;
            }

            if (!await TryCreateFolderAsync(editedExportFilename))
            {
                return null;
            }

            return editedExportFilename;
        }

        /// <summary>
        /// Checks if a folder for the given filename already exists and tries to create it.
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns>true when creating folder succeeded, or false when not</returns>
        private static async Task<bool> TryCreateFolderAsync(string filename)
        {
            string folderName = Path.GetDirectoryName(filename);
            if (Directory.Exists(folderName))
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(folderName);
            }
            catch (Exception)
            {
                // catch and ignore
            }

            if (!Directory.Exists(folderName))
            {
                await App.Current.MainPage.DisplayAlert(
                    Constants.AppTitle,
                    "Invalid path was specified: " + filename,
                    "Close");

                return false;
            }

            return true;
        }
    }
}
