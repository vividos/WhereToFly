using System;
using System.Collections.Generic;

namespace WhereToFly.Geo.SunCalcNet
{
    /// <summary>
    /// Solar times data
    /// </summary>
    public class SolarTimes
    {
        /// <summary>
        /// Solar noon, when the sun is at the highest point in the sky
        /// </summary>
        public DateTimeOffset? SolarNoon { get; internal set; }

        /// <summary>
        /// Time of the lowest point of the sun's position
        /// </summary>
        public DateTimeOffset? Nadir { get; internal set; }

        /// <summary>
        /// Time of sunrise (start), when the sun starts to appear over the horizon
        /// </summary>
        public DateTimeOffset? Sunrise { get; internal set; }

        /// <summary>
        /// Time of sunset (end), when the sun's last rays vanish from the horizon
        /// </summary>
        public DateTimeOffset? Sunset { get; internal set; }

        /// <summary>
        /// A dictionary of all sunrise and sunset times, keyed by SunTimeType. If a
        /// dictionary entry is missing, the associated sunrise/set time doesn't occur on that
        /// day.
        /// </summary>
        public IDictionary<SunTimeType, DateTimeOffset> SunriseSunsetTimes { get; set; }
    }
}
