using System;
using System.IO;
using System.Threading.Tasks;
using WhereToFly.App.Geo;
using WhereToFly.App.Geo.DataFormats;
using Xamarin.Forms;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Helper for exporting files
    /// </summary>
    public static class ExportFileHelper
    {
        /// <summary>
        /// Exports track by asking user for destination path and then using GpxWriter to write
        /// track to file
        /// </summary>
        /// <param name="track">track to export</param>
        /// <returns>task to wait on</returns>
        public static async Task ExportTrackAsync(Track track)
        {
            string exportFilename = await AskUserExportFilenameAsync(track);
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
        /// <returns>
        /// export filename, or null when editing was cancelled or an invalid path was specified
        /// </returns>
        private static async Task<string> AskUserExportFilenameAsync(Track track)
        {
            var platform = DependencyService.Get<IPlatform>();
            string exportFilename = Path.Combine(platform.PublicExportFolder, "WhereToFly", track.Name + ".gpx");

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

            string editedDirectoryName = Path.GetDirectoryName(editedExportFilename);
            if (!Directory.Exists(editedDirectoryName))
            {
                Directory.CreateDirectory(editedDirectoryName);

                if (!Directory.Exists(editedDirectoryName))
                {
                    await App.Current.MainPage.DisplayAlert(
                        Constants.AppTitle,
                        "Invalid path specified",
                        "Close");
                }

                return null;
            }

            return editedExportFilename;
        }
    }
}
