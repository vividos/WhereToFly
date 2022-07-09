using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;

namespace WhereToFly.App.Core.Services.SqliteDatabase
{
    /// <summary>
    /// Weather dashboard icon data service implementation of SQLite database data service
    /// </summary>
    internal partial class SqliteDatabaseDataService
    {
        /// <summary>
        /// Database entry for a weather dashboard icon
        /// </summary>
        [Table("weather_dashboard_icons")]
        private sealed class WeatherDashboardIconEntry
        {
            /// <summary>
            /// Weather dashboard icon ID
            /// </summary>
            [Column("id"), PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            /// <summary>
            /// Weather icon description ID
            /// </summary>
            [Column("desc_id")]
            public int WeatherIconDescriptionId { get; set; }
        }

        /// <summary>
        /// Weather dashboard icon description data service with access to the SQLite database
        /// </summary>
        private sealed class WeatherDashboardIconDataService : IWeatherIconDescriptionDataService
        {
            /// <summary>
            /// SQLite database connection
            /// </summary>
            private readonly SQLiteAsyncConnection connection;

            /// <summary>
            /// Creates a new weather dashboard icon data service
            /// </summary>
            /// <param name="connection">SQLite database connection</param>
            public WeatherDashboardIconDataService(SQLiteAsyncConnection connection)
            {
                this.connection = connection;
            }

            /// <summary>
            /// Adds a new weather icon description to the weather dashboard icon list
            /// </summary>
            /// <param name="weatherIconDescriptionToAdd">weather icon description to add</param>
            /// <returns>task to wait on</returns>
            public async Task Add(WeatherIconDescription weatherIconDescriptionToAdd)
            {
                await this.connection.InsertAsync(
                    new WeatherDashboardIconEntry
                    {
                        WeatherIconDescriptionId = await this.IdFromDescription(weatherIconDescriptionToAdd),
                    });
            }

            /// <summary>
            /// Maps a weather icon description to an ID in the table managed by this data
            /// service.
            /// </summary>
            /// <param name="weatherIconDescription">weather icon description to find</param>
            /// <returns>ID from the weather_dashboard_icons table</returns>
            private async Task<int> IdFromDescription(WeatherIconDescription weatherIconDescription)
            {
                var resultList = await this.connection.Table<WeatherIconDescriptionEntry>()
                    .Where(entry =>
                        entry.Group == weatherIconDescription.Group &&
                        entry.Name == weatherIconDescription.Name &&
                        entry.WebLink == weatherIconDescription.WebLink)
                    .ToListAsync();

                return resultList.FirstOrDefault()?.Id ?? -1;
            }

            /// <summary>
            /// Returns single weather dashboard icon by ID; currently not implemented.
            /// </summary>
            /// <param name="weatherIconDescriptionId">weather icon description ID</param>
            /// <returns>retrieved weather icon description</returns>
            public Task<WeatherIconDescription> Get(string weatherIconDescriptionId)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Removes weather dashboard icon; currently not implemented.
            /// </summary>
            /// <param name="weatherIconDescriptionId">weather icon description ID</param>
            /// <returns>task to wait on</returns>
            public Task Remove(string weatherIconDescriptionId)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Returns list of all weather dashboard icons
            /// </summary>
            /// <returns>list of weather icon description objects on the dashboard</returns>
            public async Task<IEnumerable<WeatherIconDescription>> GetList()
            {
                var weatherIconDescriptionList =
                    await this.connection.QueryAsync<WeatherIconDescriptionEntry>(
                        "select * from weather_icons where id in (select desc_id from weather_dashboard_icons)");

                return weatherIconDescriptionList.Select(entry => entry.WeatherIconDescription);
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

                var weatherIconDescriptionEntryList = new List<WeatherDashboardIconEntry>();
                foreach (var weatherIconDescription in weatherIconDescriptionList)
                {
                    weatherIconDescriptionEntryList.Add(
                        new WeatherDashboardIconEntry
                        {
                            WeatherIconDescriptionId = await this.IdFromDescription(weatherIconDescription),
                        });
                }

                await this.connection.InsertAllAsync(
                    weatherIconDescriptionEntryList,
                    runInTransaction: true);
            }

            /// <summary>
            /// Clears list of weather dashboard icons
            /// </summary>
            /// <returns>task to wait on</returns>
            public async Task ClearList()
            {
                await this.connection.DeleteAllAsync<WeatherDashboardIconEntry>();
            }
        }
    }
}
