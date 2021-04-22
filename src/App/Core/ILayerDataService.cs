using System.Collections.Generic;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Data service for Layer objects
    /// </summary>
    public interface ILayerDataService
    {
        /// <summary>
        /// Adds a new layer to the layer list
        /// </summary>
        /// <param name="layerToAdd">layer to add</param>
        /// <returns>task to wait on</returns>
        Task Add(Layer layerToAdd);

        /// <summary>
        /// Retrieves a specific layer
        /// </summary>
        /// <param name="layerId">layer ID</param>
        /// <returns>layer from list, or null when none was found</returns>
        Task<Layer> Get(string layerId);

        /// <summary>
        /// Updates an existing layer in the layer list
        /// </summary>
        /// <param name="layer">layer to update</param>
        /// <returns>task to wait on</returns>
        Task Update(Layer layer);

        /// <summary>
        /// Removes a specific layer
        /// </summary>
        /// <param name="layerId">layer ID</param>
        /// <returns>task to wait on</returns>
        Task Remove(string layerId);

        /// <summary>
        /// Returns a list of all layers
        /// </summary>
        /// <returns>list of layers</returns>
        Task<IEnumerable<Layer>> GetList();

        /// <summary>
        /// Adds new layer list
        /// </summary>
        /// <param name="layerList">layer list to add</param>
        /// <returns>task to wait on</returns>
        Task AddList(IEnumerable<Layer> layerList);

        /// <summary>
        /// Clears list of layers
        /// </summary>
        /// <returns>task to wait on</returns>
        Task ClearList();
    }
}
