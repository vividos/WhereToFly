using SQLite;
using System.Diagnostics;
using System.Text.Json;
using WhereToFly.App.Models;
using WhereToFly.App.Serializers;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Services.SqliteDatabase
{
    /// <summary>
    /// Data service implementation that stores data in an SQLite database
    /// </summary>
    internal partial class SqliteDatabaseDataService : IDataService
    {
        /// <summary>
        /// Filename for the SQLite database file
        /// </summary>
        private const string DatabaseFilename = "database.db";

        /// <summary>
        /// Key for <see cref="SecureStorage"/> to get and store app config as JSON
        /// </summary>
        private const string AppConfigSecureStorageKey = "AppConfig";

        /// <summary>
        /// SQLite database connection
        /// </summary>
        private readonly SQLiteAsyncConnection connection;

        /// <summary>
        /// Task that is completed when initializing database has completed
        /// </summary>
        private readonly Task initCompleteTask;

        /// <summary>
        /// Data service to access backend
        /// </summary>
        private readonly BackendDataService backendDataService = new();

        /// <summary>
        /// Track data service; only created once for caching purposes
        /// </summary>
        private TrackDataService? trackDataService;

        /// <summary>
        /// Backing store for app config
        /// </summary>
        private AppConfig? appConfig;

        /// <summary>
        /// Backing store for app settings
        /// </summary>
        private AppSettings? appSettings;

        #region Database entry objects
        /// <summary>
        /// Database entry for app data
        /// </summary>
        [Table("appdata")]
        internal class AppDataEntry
        {
            /// <summary>
            /// App settings object
            /// </summary>
            [Ignore]
            public AppSettings? AppSettings { get; set; }

            /// <summary>
            /// ID; needed for primary key
            /// </summary>
            [Column("id"), PrimaryKey]
            public int Id { get; set; } = 42;

            /// <summary>
            /// App settings serialized to JSON
            /// </summary>
            [Column("settings"), NotNull]
            public string Settings
            {
                get
                {
                    return JsonSerializer.Serialize(
                        this.AppSettings,
                        ModelsJsonSerializerContext.Default.AppSettings);
                }

                set
                {
                    this.AppSettings = JsonSerializer.Deserialize(
                        value,
                        ModelsJsonSerializerContext.Default.AppSettings);

                    if (this.AppSettings == null)
                    {
                        this.AppSettings = new AppSettings();
                    }

                    if (this.AppSettings.LastLocationFilterSettings == null)
                    {
                        this.AppSettings.LastLocationFilterSettings = new LocationFilterSettings();
                    }
                }
            }
        }

        /// <summary>
        /// Database entry for websites and their favicon urls
        /// </summary>a
        [Table("favicons")]
        private sealed class FaviconUrlEntry
        {
            /// <summary>
            /// Website URL
            /// </summary>
            [Column("url"), PrimaryKey]
            public string? WebsiteUrl { get; set; }

            /// <summary>
            /// Favicon address
            /// </summary>
            [Column("favicon"), NotNull]
            public string? FaviconUrl { get; set; }
        }
        #endregion

        /// <summary>
        /// Task that is completed when database initialiation has finished
        /// </summary>
        public Task InitCompleteTask => this.initCompleteTask;

        /// <summary>
        /// Creates a new data service, opens and initializes database
        /// </summary>
        public SqliteDatabaseDataService()
        {
            string folder = DeviceInfo.Platform == DevicePlatform.Unknown
                ? System.AppContext.BaseDirectory
                : FileSystem.AppDataDirectory;

            string databaseFilename = Path.Combine(
                folder,
                DatabaseFilename);

            this.connection = new SQLiteAsyncConnection(
                databaseFilename,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);

            this.initCompleteTask = Task.Run(async () => await this.InitAsync());
        }

        /// <summary>
        /// Initializes database; when the tables are newly created, add default entries.
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task InitAsync()
        {
            await this.connection.CreateTableAsync<AppDataEntry>();

            if (await this.connection.CreateTableAsync<FaviconUrlEntry>() == CreateTableResult.Created)
            {
                var defaultFaviconCache = await DataServiceHelper.GetDefaultFaviconCache();

                var defaultEntries =
                    from keyAndValue in defaultFaviconCache
                    select new FaviconUrlEntry
                    {
                        WebsiteUrl = keyAndValue.Key,
                        FaviconUrl = keyAndValue.Value,
                    };

                await this.connection.InsertAllAsync(defaultEntries, runInTransaction: true);
            }

            if (await this.connection.CreateTableAsync<LocationEntry>() == CreateTableResult.Created)
            {
                await this.GetLocationDataService().AddList(
                    DataServiceHelper.GetDefaultLocationList());
            }

            if (await this.connection.CreateTableAsync<TrackEntry>() == CreateTableResult.Created)
            {
                await this.GetTrackDataService().AddList(
                    DataServiceHelper.GetDefaultTrackList());
            }

            if (await this.connection.CreateTableAsync<LayerEntry>() == CreateTableResult.Created)
            {
                await this.GetLayerDataService().AddList(
                    await DataServiceHelper.GetInitialLayerList());
            }

            if (await this.connection.CreateTableAsync<WeatherIconDescriptionEntry>() == CreateTableResult.Created ||
                !(await this.GetWeatherIconDescriptionDataService().GetList()).Any())
            {
                await this.GetWeatherIconDescriptionDataService().AddList(
                    await DataServiceHelper.GetWeatherIconDescriptionRepository());
            }

            await this.connection.CreateTableAsync<WeatherDashboardIconEntry>();
        }

        /// <summary>
        /// Closes database again
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task CloseAsync()
        {
            await this.connection.CloseAsync();
        }

        /// <summary>
        /// Gets or retrieves the current app config object
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>app config object</returns>
        public async Task<AppConfig?> GetAppConfigAsync(CancellationToken token)
        {
            if (this.appConfig != null &&
                this.appConfig.ExpiryDate > DateTimeOffset.Now)
            {
                return this.appConfig;
            }

            // check storage first
            string? json = await SecureStorage.GetAsync(AppConfigSecureStorageKey);
            if (json != null)
            {
                this.appConfig = JsonSerializer.Deserialize(
                    json,
                    ModelsJsonSerializerContext.Default.AppConfig);
            }

            if (this.appConfig != null &&
                this.appConfig.ExpiryDate > DateTimeOffset.Now)
            {
                return this.appConfig;
            }

            // then ask backend
            try
            {
                this.appConfig = await this.backendDataService.GetAppConfigAsync(
                    AppInfo.Current.VersionString);
            }
            catch (Exception ex)
            {
                // ignore error
                Debug.WriteLine($"Error while retrieving AppConfig object: {ex}");
            }
            finally
            {
                if (this.appConfig != null)
                {
                    await SecureStorage.SetAsync(
                       AppConfigSecureStorageKey,
                       JsonSerializer.Serialize(
                           this.appConfig,
                           ModelsJsonSerializerContext.Default.AppConfig));
                }
            }

            return this.appConfig;
        }

        /// <summary>
        /// Gets the current app settings object
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <returns>app settings object</returns>
        public async Task<AppSettings> GetAppSettingsAsync(CancellationToken token)
        {
            await this.initCompleteTask;

            if (this.appSettings != null)
            {
                return this.appSettings;
            }

            var task = this.connection?
                .Table<AppDataEntry>()?
                .FirstOrDefaultAsync();

            AppDataEntry? appDataEntry = task != null ? await task : null;
            this.appSettings = appDataEntry?.AppSettings;

            if (this.appSettings == null)
            {
                this.appSettings = new AppSettings();
                await this.StoreAppSettingsAsync(this.appSettings);
            }

            return this.appSettings;
        }

        /// <summary>
        /// Stores new app settings object
        /// </summary>
        /// <param name="appSettings">new app settings to store</param>
        /// <returns>task to wait on</returns>
        public async Task StoreAppSettingsAsync(AppSettings appSettings)
        {
            await this.initCompleteTask;

            await this.connection.InsertOrReplaceAsync(new AppDataEntry
            {
                AppSettings = appSettings,
            });
        }

        /// <summary>
        /// Returns location data service that acesses the locations in the database
        /// </summary>
        /// <returns>location data service</returns>
        public ILocationDataService GetLocationDataService()
        {
            return new LocationDataService(this.connection);
        }

        /// <summary>
        /// Returns track data service that acesses the tracks in the database
        /// </summary>
        /// <returns>track data service</returns>
        public ITrackDataService GetTrackDataService()
        {
            this.trackDataService ??= new TrackDataService(this.connection);

            return this.trackDataService;
        }

        /// <summary>
        /// Returns a data service for Layer objects
        /// </summary>
        /// <returns>layer data service</returns>
        public ILayerDataService GetLayerDataService()
        {
            return new LayerDataService(this.connection);
        }

        /// <summary>
        /// Returns a data service for WeatherIconDescription objects that are available in the
        /// database. This contains a combination of a fixed repository list of icon descriptions
        /// and all user added icon descriptions.
        /// </summary>
        /// <returns>weather icon description data service</returns>
        public IWeatherIconDescriptionDataService GetWeatherIconDescriptionDataService()
        {
            return new WeatherIconDescriptionDataService(this.connection);
        }

        /// <summary>
        /// Returns a data service for WeatherIconDescription objects that are currently placed on
        /// the weather dashboard.
        /// </summary>
        /// <returns>weather icon description data service</returns>
        public IWeatherIconDescriptionDataService GetWeatherDashboardIconDataService()
        {
            return new WeatherDashboardIconDataService(this.connection);
        }

        /// <summary>
        /// Retrieves a favicon URL for the given website URL
        /// </summary>
        /// <param name="websiteUrl">website URL</param>
        /// <returns>favicon URL or empty string when none was found</returns>
        public async Task<string> GetFaviconUrlAsync(string websiteUrl)
        {
            await this.initCompleteTask;

            var uri = new Uri(websiteUrl);
            string baseUri = $"{uri.Scheme}://{uri.Host}/";

            if (uri.Host.ToLowerInvariant() == "localhost")
            {
                return $"{uri.Scheme}://{uri.Host}/favicon.ico";
            }

            string? faviconUrl =
                (await this.connection.FindAsync<FaviconUrlEntry>(baseUri))?.FaviconUrl;

            if (!string.IsNullOrEmpty(faviconUrl))
            {
                return faviconUrl!;
            }

            try
            {
                faviconUrl = await this.backendDataService.GetFaviconUrlAsync(websiteUrl);

                await this.connection.InsertAsync(new FaviconUrlEntry
                {
                    WebsiteUrl = baseUri,
                    FaviconUrl = faviconUrl,
                });

                return faviconUrl;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Retrieves latest info about a live waypoint, including new coordinates and
        /// description.
        /// </summary>
        /// <param name="liveWaypointId">live waypoint ID</param>
        /// <returns>query result for live waypoint</returns>
        public async Task<LiveWaypointQueryResult> GetLiveWaypointDataAsync(string liveWaypointId)
        {
            return await this.backendDataService.GetLiveWaypointDataAsync(liveWaypointId);
        }

        /// <summary>
        /// Retrieves latest info about a live track, including new list of track points and
        /// description.
        /// </summary>
        /// <param name="liveTrackId">live track ID</param>
        /// <param name="lastTrackPointTime">
        /// last track point that the client already has received, or null when no track points
        /// are known yet
        /// </param>
        /// <returns>query result for live track</returns>
        public async Task<LiveTrackQueryResult> GetLiveTrackDataAsync(
            string liveTrackId,
            DateTimeOffset? lastTrackPointTime)
        {
            return await this.backendDataService.GetLiveTrackDataAsync(liveTrackId, lastTrackPointTime);
        }

        /// <summary>
        /// Plans a tour with given tour planning parameters and returns the planned tour.
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <returns>planned tour</returns>
        public async Task<PlannedTour> PlanTourAsync(PlanTourParameters planTourParameters)
        {
            return await this.backendDataService.PlanTourAsync(planTourParameters);
        }
    }
}
