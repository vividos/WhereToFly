using Newtonsoft.Json;
using SQLite;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Services.SqliteDatabase
{
    /// <summary>
    /// Location data service implementation of SQLite database data service
    /// </summary>
    internal partial class SqliteDatabaseDataService
    {
        /// <summary>
        /// Database entry for a location
        /// </summary>
        [Table("locations")]
        private sealed class LocationEntry
        {
            /// <summary>
            /// Location to store in the entry
            /// </summary>
            [Ignore]
            public Location Location { get; }

            /// <summary>
            /// Location ID
            /// </summary>
            [Column("id"), PrimaryKey]
            public string Id
            {
                get => this.Location.Id;
                set => this.Location.Id = value;
            }

            /// <summary>
            /// Location name
            /// </summary>
            [Column("name"), Indexed, Collation("nocase")]
            public string Name
            {
                get => this.Location.Name;
                set => this.Location.Name = value;
            }

            /// <summary>
            /// Map location
            /// </summary>
            [Column("map_location")]
            public string MapLocation
            {
                get => JsonConvert.SerializeObject(this.Location.MapLocation);
                set => this.Location.MapLocation = JsonConvert.DeserializeObject<MapPoint>(value);
            }

            /// <summary>
            /// Location description
            /// </summary>
            [Column("desc"), Indexed, Collation("nocase")]
            public string Description
            {
                get => this.Location.Description;
                set => this.Location.Description = value;
            }

            /// <summary>
            /// Location type
            /// </summary>
            [Column("type")]
            public LocationType Type
            {
                get => this.Location.Type;
                set => this.Location.Type = value;
            }

            /// <summary>
            /// Takeoff directions
            /// </summary>
            [Column("takeoff_dir")]
            public TakeoffDirections TakeoffDirections
            {
                get => this.Location.TakeoffDirections;
                set => this.Location.TakeoffDirections = value;
            }

            /// <summary>
            /// Internet link
            /// </summary>
            [Column("internet_link"), Collation("nocase")]
            public string InternetLink
            {
                get => this.Location.InternetLink;
                set => this.Location.InternetLink = value;
            }

            /// <summary>
            /// Is location a plan tour location
            /// </summary>
            [Column("is_plan_tour_location")]
            public bool IsPlanTourLocation
            {
                get => this.Location.IsPlanTourLocation;
                set => this.Location.IsPlanTourLocation = value;
            }

            /// <summary>
            /// Creates an empty location entry; used when loading entry from database
            /// </summary>
            public LocationEntry()
            {
                this.Location = new Location();
            }

            /// <summary>
            /// Creates a new entry from given location
            /// </summary>
            /// <param name="location">location to use</param>
            public LocationEntry(Location location)
            {
                this.Location = location;
            }
        }

        /// <summary>
        /// Location data service with access to the SQLite database
        /// </summary>
        private sealed class LocationDataService : ILocationDataService
        {
            /// <summary>
            /// SQLite database connection
            /// </summary>
            private readonly SQLiteAsyncConnection connection;

            /// <summary>
            /// Creates a new location data service
            /// </summary>
            /// <param name="connection">SQLite database connection</param>
            internal LocationDataService(SQLiteAsyncConnection connection)
            {
                this.connection = connection;
            }

            /// <summary>
            /// Adds a new location to the location list
            /// </summary>
            /// <param name="locationToAdd">location to add</param>
            /// <returns>task to wait on</returns>
            public async Task Add(Location locationToAdd)
            {
                await this.connection.InsertAsync(new LocationEntry(locationToAdd));
            }

            /// <summary>
            /// Retrieves a specific location
            /// </summary>
            /// <param name="locationId">location ID</param>
            /// <returns>location from list, or null when none was found</returns>
            public async Task<Location> Get(string locationId)
            {
                return (await this.connection.GetAsync<LocationEntry>(locationId))?.Location;
            }

            /// <summary>
            /// Updates an existing location in the location list
            /// </summary>
            /// <param name="location">location to update</param>
            /// <returns>task to wait on</returns>
            public async Task Update(Location location)
            {
                await this.connection.UpdateAsync(new LocationEntry(location));
            }

            /// <summary>
            /// Removes a specific location
            /// </summary>
            /// <param name="locationId">location ID</param>
            /// <returns>task to wait on</returns>
            public async Task Remove(string locationId)
            {
                await this.connection.DeleteAsync<LocationEntry>(locationId);
            }

            /// <summary>
            /// Returns if the location list is empty
            /// </summary>
            /// <returns>true when list is empty, false when not</returns>
            public async Task<bool> IsListEmpty()
            {
                return await this.connection.Table<LocationEntry>().CountAsync() == 0;
            }

            /// <summary>
            /// Returns a list of all locations, possibly filtered
            /// </summary>
            /// <param name="filterSettings">filter settings; may be null</param>
            /// <returns>list of locations</returns>
            public async Task<IEnumerable<Location>> GetList(
                LocationFilterSettings filterSettings = null)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                List<LocationEntry> locationEntryList = null;
                if (filterSettings == null ||
                    (string.IsNullOrEmpty(filterSettings.FilterText) &&
                     TakeoffDirectionsHelper.ModifyAdjacentDirectionsFromView(
                         filterSettings.FilterTakeoffDirections) == TakeoffDirections.All &&
                     filterSettings.ShowNonTakeoffLocations))
                {
                    locationEntryList = await this.connection.Table<LocationEntry>().ToListAsync();
                }
                else
                {
                    locationEntryList = await this.GetLocationListFromFilterSettings(filterSettings);
                }

                stopwatch.Stop();

                Debug.WriteLine($"Location query with params {filterSettings?.ToString()} took {stopwatch.ElapsedMilliseconds} ms");

                if (locationEntryList == null ||
                    !locationEntryList.Any())
                {
                    return Enumerable.Empty<Location>();
                }

                return locationEntryList.Select(locationEntry => locationEntry.Location);
            }

            /// <summary>
            /// Gets a location list filtered by the given filter settiings
            /// </summary>
            /// <param name="filterSettings">filter settings to use</param>
            /// <returns>list of zero or more location entries</returns>
            private async Task<List<LocationEntry>> GetLocationListFromFilterSettings(LocationFilterSettings filterSettings)
            {
                var builder = new SqlQueryBuilder("locations");

                if (!string.IsNullOrEmpty(filterSettings.FilterText))
                {
                    string filterText = "%" + filterSettings.FilterText + "%";
                    builder.AddWhereClause(
                        "(name like ? or desc like ?)",
                        filterText,
                        filterText);
                }

                TakeoffDirections directions = TakeoffDirectionsHelper.ModifyAdjacentDirectionsFromView(
                    filterSettings.FilterTakeoffDirections);

                if (directions != TakeoffDirections.All)
                {
                    string queryText =
                        filterSettings.ShowNonTakeoffLocations
                        ? "(takeoff_dir & ? != 0 or type != ?)"
                        : "takeoff_dir & ? != 0 and type = ?";

                    builder.AddWhereClause(
                        queryText,
                        (int)directions,
                        (int)LocationType.FlyingTakeoff);
                }
                else
                {
                    if (!filterSettings.ShowNonTakeoffLocations)
                    {
                        builder.AddWhereClause(
                            "type = ?",
                            (int)LocationType.FlyingTakeoff);
                    }
                }

                var locationEntryList = await this.connection.QueryAsync<LocationEntry>(
                    builder.Build(),
                    builder.BoundObjects);

                return locationEntryList;
            }

            /// <summary>
            /// Adds new location list
            /// </summary>
            /// <param name="locationList">location list to add</param>
            /// <returns>task to wait on</returns>
            public async Task AddList(IEnumerable<Location> locationList)
            {
                if (!locationList.Any())
                {
                    return;
                }

                var locationEntryList =
                    from location in locationList
                    select new LocationEntry(location);

                await this.connection.InsertAllAsync(locationEntryList, runInTransaction: true);
            }

            /// <summary>
            /// Clears list of locations
            /// </summary>
            /// <returns>task to wait on</returns>
            public async Task ClearList()
            {
                await this.connection.DropTableAsync<LocationEntry>();
                await this.connection.CreateTableAsync<LocationEntry>();
            }
        }
    }
}
