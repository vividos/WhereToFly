using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;
using Xamarin.Essentials;

namespace WhereToFly.App.Core.Services.SqliteDatabase
{
    /// <summary>
    /// Track data service implementation of SQLite database data service
    /// </summary>
    internal partial class SqliteDatabaseDataService
    {
        /// <summary>
        /// Folder name for all tracks files
        /// </summary>
        private const string TracksFolderName = "tracks";

        /// <summary>
        /// Database entry for a track
        /// </summary>
        [Table("tracks")]
        private sealed class TrackEntry
        {
            /// <summary>
            /// Track to store in the entry
            /// </summary>
            [Ignore]
            public Track Track { get; set; }

            /// <summary>
            /// Track ID
            /// </summary>
            [Column("id"), PrimaryKey]
            public string Id
            {
                get => this.Track.Id;
                set => this.Track.Id = value;
            }

            /// <summary>
            /// Track name
            /// </summary>
            [Column("name")]
            public string Name
            {
                get => this.Track.Name;
                set => this.Track.Name = value;
            }

            /// <summary>
            /// Track description
            /// </summary>
            [Column("desc")]
            public string Description
            {
                get => this.Track.Description;
                set => this.Track.Description = value;
            }

            /// <summary>
            /// Track is a flight?
            /// </summary>
            [Column("is_flight")]
            public bool IsFlightTrack
            {
                get => this.Track.IsFlightTrack;
                set => this.Track.IsFlightTrack = value;
            }

            /// <summary>
            /// Track is a live track?
            /// </summary>
            [Column("is_livetrack")]
            public bool IsLiveTrack
            {
                get => this.Track.IsLiveTrack;
                set => this.Track.IsLiveTrack = value;
            }

            /// <summary>
            /// Track color
            /// </summary>
            [Column("color")]
            public string Color
            {
                get => this.Track.Color;
                set => this.Track.Color = value;
            }

            /// <summary>
            /// Duration of track
            /// </summary>
            [Column("duration")]
            public TimeSpan Duration
            {
                get => this.Track.Duration;
                set => this.Track.Duration = value;
            }

            /// <summary>
            /// Length of track, in meter
            /// </summary>
            [Column("length")]
            public double LengthInMeter
            {
                get => this.Track.LengthInMeter;
                set => this.Track.LengthInMeter = value;
            }

            /// <summary>
            /// Height gain, in meter
            /// </summary>
            [Column("height_gain")]
            public double HeightGain
            {
                get => this.Track.HeightGain;
                set => this.Track.HeightGain = value;
            }

            /// <summary>
            /// Height loss, in meter
            /// </summary>
            [Column("height_loss")]
            public double HeightLoss
            {
                get => this.Track.HeightLoss;
                set => this.Track.HeightLoss = value;
            }

            /// <summary>
            /// Max. height, in meter
            /// </summary>
            [Column("max_height")]
            public double MaxHeight
            {
                get => this.Track.MaxHeight;
                set => this.Track.MaxHeight = value;
            }

            /// <summary>
            /// Min. height, in meter
            /// </summary>
            [Column("min_height")]
            public double MinHeight
            {
                get => this.Track.MinHeight;
                set => this.Track.MinHeight = value;
            }

            /// <summary>
            /// Max. climb rate, in m/s
            /// </summary>
            [Column("max_climb_rate")]
            public double MaxClimbRate
            {
                get => this.Track.MaxClimbRate;
                set => this.Track.MaxClimbRate = value;
            }

            /// <summary>
            /// Max. sink rate (or min. negative climb rate), in m/s
            /// </summary>
            [Column("max_sink_rate")]
            public double MaxSinkRate
            {
                get => this.Track.MaxSinkRate;
                set => this.Track.MaxSinkRate = value;
            }

            /// <summary>
            /// Maximum speed, in km/h
            /// </summary>
            [Column("max_speed")]
            public double MaxSpeed
            {
                get => this.Track.MaxSpeed;
                set => this.Track.MaxSpeed = value;
            }

            /// <summary>
            /// Average speed, in km/h
            /// </summary>
            [Column("average_speed")]
            public double AverageSpeed
            {
                get => this.Track.AverageSpeed;
                set => this.Track.AverageSpeed = value;
            }

            /// <summary>
            /// Track points are stored in internal storage instead of in the database
            /// </summary>
            [Column("filename")]
            public string TrackPointFilename { get; set; }

            /// <summary>
            /// Ground height profile
            /// </summary>
            [Column("ground_height_profile")]
            public string GroundHeightProfile
            {
                get => JsonConvert.SerializeObject(this.Track.GroundHeightProfile);
                set
                {
                    var list = value == null
                        ? null
                        : JsonConvert.DeserializeObject<List<double>>(value);
                    this.Track.GroundHeightProfile = list ?? new List<double>();
                }
            }

            /// <summary>
            /// Creates an empty track entry; used when loading entry from database
            /// </summary>
            public TrackEntry()
            {
                this.Track = new Track();
            }

            /// <summary>
            /// Creates a new entry from given track
            /// </summary>
            /// <param name="track">track to use</param>
            public TrackEntry(Track track)
            {
                this.Track = track;
            }

            /// <summary>
            /// Loads track points from internal storage
            /// </summary>
            public void LoadTrackPoints()
            {
                Debug.Assert(
                    !string.IsNullOrEmpty(this.TrackPointFilename),
                    "must have set track point filename when loading a track");

                string filename = Path.Combine(
                    FileSystem.AppDataDirectory,
                    TracksFolderName,
                    this.TrackPointFilename);

                string json = File.ReadAllText(filename);
                this.Track.TrackPoints = JsonConvert.DeserializeObject<List<TrackPoint>>(
                    json,
                    new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new TrackPoint.Converter() },
                    });
            }

            /// <summary>
            /// Stores track points to internal storage, generating a new track filename if
            /// necessary.
            /// </summary>
            public void StoreTrackPoints()
            {
                string tracksFolder = Path.Combine(
                    FileSystem.AppDataDirectory,
                    TracksFolderName);

                if (!Directory.Exists(tracksFolder))
                {
                    Directory.CreateDirectory(tracksFolder);
                }

                if (string.IsNullOrEmpty(this.TrackPointFilename))
                {
                    this.TrackPointFilename = Guid.NewGuid().ToString("B") + ".json";
                }

                string filename = Path.Combine(tracksFolder, this.TrackPointFilename);

                string json = JsonConvert.SerializeObject(
                    this.Track.TrackPoints,
                    new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new TrackPoint.Converter() },
                    });

                File.WriteAllText(filename, json);
            }
        }

        /// <summary>
        /// Track data service with access to the SQLite database
        /// </summary>
        private sealed class TrackDataService : ITrackDataService
        {
            /// <summary>
            /// SQLite database connection
            /// </summary>
            private readonly SQLiteAsyncConnection connection;

            /// <summary>
            /// Cache for tracks, with track ID as key
            /// </summary>
            private readonly Dictionary<string, Track> trackCache = new();

            /// <summary>
            /// Creates a new track data service
            /// </summary>
            /// <param name="connection">SQLite database connection</param>
            public TrackDataService(SQLiteAsyncConnection connection)
            {
                this.connection = connection;
            }

            /// <summary>
            /// Adds a new track to the track list
            /// </summary>
            /// <param name="trackToAdd">track to add</param>
            /// <returns>task to wait on</returns>
            public async Task Add(Track trackToAdd)
            {
                var trackEntry = new TrackEntry(trackToAdd);

                trackEntry.StoreTrackPoints();

                await this.connection.InsertAsync(trackEntry);

                this.trackCache[trackToAdd.Id] = trackToAdd;
            }

            /// <summary>
            /// Retrieves a specific track
            /// </summary>
            /// <param name="trackId">track ID</param>
            /// <returns>track from list, or null when none was found</returns>
            public async Task<Track> Get(string trackId)
            {
                if (this.trackCache.ContainsKey(trackId))
                {
                    return this.trackCache[trackId];
                }

                var trackEntry = await this.connection.GetAsync<TrackEntry>(trackId);

                trackEntry.LoadTrackPoints();

                return trackEntry?.Track;
            }

            /// <summary>
            /// Updates an existing track
            /// </summary>
            /// <param name="trackToUpdate">track to update</param>
            /// <returns>task to wait on</returns>
            public async Task Update(Track trackToUpdate)
            {
                var trackEntry = await this.connection.GetAsync<TrackEntry>(trackToUpdate.Id);

                trackEntry.Track = trackToUpdate;

                await this.connection.UpdateAsync(trackEntry);

                this.trackCache[trackToUpdate.Id] = trackToUpdate;
            }

            /// <summary>
            /// Removes a specific track
            /// </summary>
            /// <param name="trackId">track ID</param>
            /// <returns>task to wait on</returns>
            public async Task Remove(string trackId)
            {
                var trackEntry = await this.connection.GetAsync<TrackEntry>(trackId);

                string filename = trackEntry.TrackPointFilename;
                File.Delete(filename);

                await this.connection.DeleteAsync<TrackEntry>(trackId);

                this.trackCache.Remove(trackId);
            }

            /// <summary>
            /// Returns a list of all tracks
            /// </summary>
            /// <returns>list of tracks</returns>
            public async Task<IEnumerable<Track>> GetList()
            {
                var trackList = await this.connection.Table<TrackEntry>().ToListAsync();

                foreach (var trackEntry in trackList)
                {
                    if (this.trackCache.ContainsKey(trackEntry.Id))
                    {
                        trackEntry.Track = this.trackCache[trackEntry.Id];
                    }
                    else
                    {
                        trackEntry.LoadTrackPoints();
                    }
                }

                return trackList.Select(trackEntry => trackEntry.Track);
            }

            /// <summary>
            /// Adds new track list
            /// </summary>
            /// <param name="trackList">track list to add</param>
            /// <returns>task to wait on</returns>
            public async Task AddList(IEnumerable<Track> trackList)
            {
                if (!trackList.Any())
                {
                    return;
                }

                var trackEntryList =
                    (from track in trackList
                     select new TrackEntry(track)).ToList();

                foreach (var trackEntry in trackEntryList)
                {
                    trackEntry.StoreTrackPoints();
                }

                await this.connection.InsertAllAsync(trackEntryList, runInTransaction: true);

                foreach (var trackEntry in trackEntryList)
                {
                    this.trackCache[trackEntry.Id] = trackEntry.Track;
                }
            }

            /// <summary>
            /// Clears list of tracks
            /// </summary>
            /// <returns>task to wait on</returns>
            public async Task ClearList()
            {
                var trackEntryList = await this.connection.QueryAsync<TrackEntry>(
                    "select filename from tracks");

                trackEntryList.ToList().ForEach(trackEntry => File.Delete(trackEntry.TrackPointFilename));

                await this.connection.DeleteAllAsync<TrackEntry>();

                ClearTrackFolder();

                this.trackCache.Clear();
            }

            /// <summary>
            /// Clears track folder by removing all files and the folder itself
            /// </summary>
            private static void ClearTrackFolder()
            {
                try
                {
                    string tracksFolder = Path.Combine(
                        FileSystem.AppDataDirectory,
                        TracksFolderName);

                    if (Directory.Exists(tracksFolder))
                    {
                        Directory.Delete(tracksFolder, true);
                    }
                }
                catch (Exception)
                {
                    // ignore errors
                }
            }
        }
    }
}
