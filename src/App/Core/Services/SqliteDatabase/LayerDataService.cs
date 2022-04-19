using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Services.SqliteDatabase
{
    /// <summary>
    /// Layer data service implementation of SQLite database data service
    /// </summary>
    internal partial class SqliteDatabaseDataService
    {
        /// <summary>
        /// Database entry for a layer
        /// </summary>
        [Table("layers")]
        private sealed class LayerEntry
        {
            /// <summary>
            /// Layer to store in the entry
            /// </summary>
            [Ignore]
            public Layer Layer { get; set; }

            /// <summary>
            /// Layer ID
            /// </summary>
            [Column("id"), PrimaryKey]
            public string Id
            {
                get => this.Layer.Id;
                set => this.Layer.Id = value;
            }

            /// <summary>
            /// Layer name
            /// </summary>
            [Column("name")]
            public string Name
            {
                get => this.Layer.Name;
                set => this.Layer.Name = value;
            }

            /// <summary>
            /// Layer description
            /// </summary>
            [Column("desc")]
            public string Description
            {
                get => this.Layer.Description;
                set => this.Layer.Description = value;
            }

            /// <summary>
            /// Layer type
            /// </summary>
            [Column("type")]
            public LayerType LayerType
            {
                get => this.Layer.LayerType;
                set => this.Layer.LayerType = value;
            }

            /// <summary>
            /// Layer visibility
            /// </summary>
            [Column("visible")]
            public bool IsVisible
            {
                get => this.Layer.IsVisible;
                set => this.Layer.IsVisible = value;
            }

            /// <summary>
            /// Layer data
            /// </summary>
            [Column("data")]
            public string Data
            {
                get => this.Layer.Data;
                set => this.Layer.Data = value;
            }

            /// <summary>
            /// Creates an empty layer entry; used when loading entry from database
            /// </summary>
            public LayerEntry()
            {
                this.Layer = new Layer();
            }

            /// <summary>
            /// Creates a new entry from given layer
            /// </summary>
            /// <param name="layer">layer to use</param>
            public LayerEntry(Layer layer)
            {
                this.Layer = layer;
            }
        }

        /// <summary>
        /// Layer data service with access to the SQLite database
        /// </summary>
        private sealed class LayerDataService : ILayerDataService
        {
            /// <summary>
            /// SQLite database connection
            /// </summary>
            private readonly SQLiteAsyncConnection connection;

            /// <summary>
            /// Creates a new layer data service
            /// </summary>
            /// <param name="connection">SQLite database connection</param>
            public LayerDataService(SQLiteAsyncConnection connection)
            {
                this.connection = connection;
            }

            /// <summary>
            /// Adds a new layer to the layer list
            /// </summary>
            /// <param name="layerToAdd">layer to add</param>
            /// <returns>task to wait on</returns>
            public async Task Add(Layer layerToAdd)
            {
                await this.connection.InsertAsync(new LayerEntry(layerToAdd));
            }

            /// <summary>
            /// Retrieves a specific layer
            /// </summary>
            /// <param name="layerId">layer ID</param>
            /// <returns>layer from list, or null when none was found</returns>
            public async Task<Layer> Get(string layerId)
            {
                var layerEntry = await this.connection.GetAsync<LayerEntry>(layerId);

                return layerEntry?.Layer;
            }

            /// <summary>
            /// Updates an existing layer in the layer list
            /// </summary>
            /// <param name="layer">layer to update</param>
            /// <returns>task to wait on</returns>
            public async Task Update(Layer layer)
            {
                await this.connection.UpdateAsync(new LayerEntry(layer));
            }

            /// <summary>
            /// Removes a specific layer
            /// </summary>
            /// <param name="layerId">layer ID</param>
            /// <returns>task to wait on</returns>
            public async Task Remove(string layerId)
            {
                await this.connection.DeleteAsync<LayerEntry>(layerId);
            }

            /// <summary>
            /// Returns a list of all layers
            /// </summary>
            /// <returns>list of layers</returns>
            public async Task<IEnumerable<Layer>> GetList()
            {
                var layerList = await this.connection.Table<LayerEntry>().ToListAsync();
                return layerList.Select(layerEntry => layerEntry.Layer);
            }

            /// <summary>
            /// Adds new layer list
            /// </summary>
            /// <param name="layerList">layer list to add</param>
            /// <returns>task to wait on</returns>
            public async Task AddList(IEnumerable<Layer> layerList)
            {
                if (!layerList.Any())
                {
                    return;
                }

                var layerEntryList =
                    from layer in layerList
                    select new LayerEntry(layer);

                await this.connection.InsertAllAsync(layerEntryList, runInTransaction: true);
            }

            /// <summary>
            /// Clears list of layers
            /// </summary>
            /// <returns>task to wait on</returns>
            public async Task ClearList()
            {
                await this.connection.DeleteAllAsync<LayerEntry>();
            }
        }
    }
}
