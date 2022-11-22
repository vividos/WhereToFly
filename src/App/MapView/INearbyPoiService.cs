using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Service to find and access nearby POIs
    /// </summary>
    public interface INearbyPoiService
    {
        /// <summary>
        /// Returns list of nearby POIs in the given map area
        /// </summary>
        /// <param name="area">map area to find POIs in</param>
        /// <param name="visiblePoiIds">list of POIs already visible in the area</param>
        /// <returns>list of new locations</returns>
        Task<IEnumerable<Location>> Get(MapRectangle area, IEnumerable<string> visiblePoiIds);
    }
}
