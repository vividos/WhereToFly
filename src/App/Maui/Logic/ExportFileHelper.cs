using WhereToFly.App.Abstractions;
using WhereToFly.Geo.DataFormats;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Logic
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
            var appMapService = DependencyService.Get<IAppMapService>();
            byte[]? data = await appMapService.MapView.ExportLayerAsync(layer);
            if (data == null)
            {
                var userInterface = DependencyService.Get<IUserInterface>();

                userInterface.DisplayToast("Error occured at layer export.");
                return;
            }

            string? exportFilename = await AskUserExportFilenameAsync(layer.Name + ".kmz");
            if (exportFilename == null)
            {
                return;
            }

            string exportPath = Path.Combine(FileSystem.CacheDirectory, exportFilename);

            try
            {
                await File.WriteAllBytesAsync(exportPath, data);

                await ShareFile(exportPath, "application/vnd.google-earth.kmz");
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                var userInterface = DependencyService.Get<IUserInterface>();

                await userInterface.DisplayAlert(
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
            string? exportFilename = await AskUserExportFilenameAsync(track.Name + ".gpx");
            if (exportFilename == null)
            {
                return;
            }

            string exportPath = Path.Combine(FileSystem.CacheDirectory, exportFilename);

            try
            {
                GpxWriter.WriteTrack(exportPath, track);

                await ShareFile(exportPath, "application/gpx+xml");
            }
            catch (Exception ex)
            {
                App.LogError(ex);

                var userInterface = DependencyService.Get<IUserInterface>();

                await userInterface.DisplayAlert(
                    "Error while exporting track: " + ex.Message,
                    "OK");
            }
        }

        /// <summary>
        /// Ask user about the export filename and return it
        /// </summary>
        /// <param name="exportFilename">filename and extension of file to export</param>
        /// <returns>
        /// export filename, or null when editing was cancelled or an invalid path was specified
        /// </returns>
        private static async Task<string?> AskUserExportFilenameAsync(string exportFilename)
        {
            string editedExportFilename = await UserInterface.MainPage.DisplayPromptAsync(
                Constants.AppTitle,
                "Export filename",
                "Export",
                "Cancel",
                initialValue: exportFilename);

            if (string.IsNullOrEmpty(editedExportFilename))
            {
                return null;
            }

            return editedExportFilename;
        }

        /// <summary>
        /// Shares file with other apps
        /// </summary>
        /// <param name="filename">full fulename of file to share</param>
        /// <param name="contentType">MIME type of content</param>
        /// <returns>task to wait on</returns>
        private static async Task ShareFile(string filename, string contentType)
        {
            await Share.RequestAsync(
                new ShareFileRequest(
                    $"Sharing file {Path.GetFileName(filename)}...",
                    new ShareFile(filename, contentType)));
        }
    }
}
