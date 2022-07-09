using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;

namespace WhereToFly.App.Core.Services.SqliteDatabase
{
    /// <summary>
    /// Weather icon description data service implementation of SQLite database data service
    /// </summary>
    internal partial class SqliteDatabaseDataService
    {
        /// <summary>
        /// Database entry for a weather icon description
        /// </summary>
        [Table("weather_icons")]
        private sealed class WeatherIconDescriptionEntry
        {
            /// <summary>
            /// Weather icon description to store in the entry
            /// </summary>
            [Ignore]
            public WeatherIconDescription WeatherIconDescription { get; set; }

            /// <summary>
            /// Weather icon description ID
            /// </summary>
            [Column("id"), PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            /// <summary>
            /// Weather icon description name
            /// </summary>
            [Column("name")]
            public string Name
            {
                get => this.WeatherIconDescription.Name;
                set => this.WeatherIconDescription.Name = value;
            }

            /// <summary>
            /// Weather icon description group
            /// </summary>
            [Column("group")]
            public string Group
            {
                get => this.WeatherIconDescription.Group;
                set => this.WeatherIconDescription.Group = value;
            }

            /// <summary>
            /// Weather icon description type
            /// </summary>
            [Column("type")]
            public WeatherIconDescription.IconType Type
            {
                get => this.WeatherIconDescription.Type;
                set => this.WeatherIconDescription.Type = value;
            }

            /// <summary>
            /// Weather icon description weblink
            /// </summary>
            [Column("weblink")]
            public string WebLink
            {
                get => this.WeatherIconDescription.WebLink;
                set => this.WeatherIconDescription.WebLink = value;
            }

            /// <summary>
            /// Creates an empty weather icon description entry; used when loading entry from database
            /// </summary>
            public WeatherIconDescriptionEntry()
            {
                this.WeatherIconDescription = new WeatherIconDescription();
            }

            /// <summary>
            /// Creates a new entry from given weather icon description
            /// </summary>
            /// <param name="weatherIconDescription">weather icon description</param>
            public WeatherIconDescriptionEntry(WeatherIconDescription weatherIconDescription)
            {
                this.WeatherIconDescription = weatherIconDescription;
            }
        }

        /// <summary>
        /// Weather icon description data service with access to the SQLite database
        /// </summary>
        private sealed class WeatherIconDescriptionDataService : IWeatherIconDescriptionDataService
        {
            /// <summary>
            /// SQLite database connection
            /// </summary>
            private readonly SQLiteAsyncConnection connection;

            /// <summary>
            /// Creates a new weather icon description data service
            /// </summary>
            /// <param name="connection">SQLite database connection</param>
            public WeatherIconDescriptionDataService(SQLiteAsyncConnection connection)
            {
                this.connection = connection;
            }

            /// <summary>
            /// Adds a new weather icon description to the weather icon description list
            /// </summary>
            /// <param name="weatherIconDescriptionToAdd">weather icon description to add</param>
            /// <returns>task to wait on</returns>
            public async Task Add(WeatherIconDescription weatherIconDescriptionToAdd)
            {
                await this.connection.InsertAsync(new WeatherIconDescriptionEntry(weatherIconDescriptionToAdd));
            }

            /// <summary>
            /// Retrieves a specific weather icon description
            /// </summary>
            /// <param name="weatherIconDescriptionId">weather icon description ID</param>
            /// <returns>weather icon description from list, or null when none was found</returns>
            public async Task<WeatherIconDescription> Get(string weatherIconDescriptionId)
            {
                var weatherIconDescriptionEntry =
                    await this.connection.GetAsync<WeatherIconDescriptionEntry>(weatherIconDescriptionId);

                return weatherIconDescriptionEntry?.WeatherIconDescription;
            }

            /// <summary>
            /// Removes a specific weather icon description
            /// </summary>
            /// <param name="weatherIconDescriptionId">weather icon description ID</param>
            /// <returns>task to wait on</returns>
            public async Task Remove(string weatherIconDescriptionId)
            {
                await this.connection.DeleteAsync<WeatherIconDescriptionEntry>(weatherIconDescriptionId);
            }

            /// <summary>
            /// Returns a list of all weather icon descriptions
            /// </summary>
            /// <returns>list of weather icon descriptions</returns>
            public async Task<IEnumerable<WeatherIconDescription>> GetList()
            {
                var weatherIconDescriptionList =
                    await this.connection.Table<WeatherIconDescriptionEntry>()
                    .ToListAsync();

                return weatherIconDescriptionList.Select(
                    weatherIconDescriptionEntry => weatherIconDescriptionEntry.WeatherIconDescription);
            }

            /// <summary>
            /// Adds new weather icon description list
            /// </summary>
            /// <param name="weatherIconDescriptionList">weather icon description list to add</param>
            /// <returns>task to wait on</returns>
            public async Task AddList(IEnumerable<WeatherIconDescription> weatherIconDescriptionList)
            {
                if (!weatherIconDescriptionList.Any())
                {
                    return;
                }

                var weatherIconDescriptionEntryList =
                    from weatherIconDescription in weatherIconDescriptionList
                    select new WeatherIconDescriptionEntry(weatherIconDescription);

                await this.connection.InsertAllAsync(weatherIconDescriptionEntryList, runInTransaction: true);
            }

            /// <summary>
            /// Clears list of weather icon descriptions and re-adds the default list
            /// </summary>
            /// <returns>task to wait on</returns>
            public async Task ClearList()
            {
                await this.connection.DeleteAllAsync<WeatherIconDescriptionEntry>();

                await this.AddList(DataServiceHelper.GetWeatherIconDescriptionRepository());
            }
        }
    }
}
