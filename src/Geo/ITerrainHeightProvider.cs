using System.Threading.Tasks;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Interface to a provider for terrain height infos
    /// </summary>
    public interface ITerrainHeightProvider
    {
        /// <summary>
        /// Samples height profile points for a track
        /// </summary>
        /// <param name="track">track object</param>
        /// <returns>list of terrain height values for given track points</returns>
        Task<double[]> SampleTrackHeights(Track track);
    }
}
