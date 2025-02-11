using WhereToFly.Geo.Model;

namespace WhereToFly.App
{
    /// <summary>
    /// Extension methods for the app
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Converts a geo location to <see cref="MapPoint"/>
        /// </summary>
        /// <param name="location">geo location object; must not be null</param>
        /// <returns>map point</returns>
        public static MapPoint ToMapPoint(this Microsoft.Maui.Devices.Sensors.Location location)
        {
            return new MapPoint(
                location.Latitude,
                location.Longitude,
                location.Altitude);
        }

        /// <summary>
        /// When a task throws an exception (technically, when it is faulted),
        /// observes the exception and logs it.
        /// </summary>
        /// <param name="task">task to log exception for</param>
        /// <returns>chained task object</returns>
        public static Task LogTaskException(this Task task)
        {
            return task.ContinueWith(
                faultedTask =>
                {
                    if (faultedTask.Exception != null)
                    {
                        App.LogError(faultedTask.Exception);
                    }
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
