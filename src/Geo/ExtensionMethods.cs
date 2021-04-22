using System;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Extension methods for spatial classes
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts an angle in degrees to radians
        /// </summary>
        /// <param name="angleInDegrees">angle to convert</param>
        /// <returns>angle in radians</returns>
        public static double ToRadians(this double angleInDegrees)
        {
            return angleInDegrees * (Math.PI / 180);
        }

        /// <summary>
        /// Converts an angle in radians to degrees
        /// </summary>
        /// <param name="angleInRadians">angle to convert</param>
        /// <returns>angle in degrees</returns>
        public static double ToDegrees(this double angleInRadians)
        {
            return angleInRadians * (180 / Math.PI);
        }
    }
}
