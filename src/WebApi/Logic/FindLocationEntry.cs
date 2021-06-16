using Newtonsoft.Json;
using SQLite;
using WhereToFly.Geo.Model;

namespace WhereToFly.WebApi.Logic
{
    /// <summary>
    /// Database entry for a location
    /// </summary>
    [Table("locations")]
    internal class FindLocationEntry
    {
        /// <summary>
        /// Location to store in the entry
        /// </summary>
        [Ignore]
        public Location Location { get; set; }

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
        /// Latitude of location
        /// </summary>
        [Column("latitude"), Indexed]
        public double Latitude
        {
            get => this.Location.MapLocation?.Latitude ?? 0.0;
            set => this.Location.MapLocation = new MapPoint(latitude: value, longitude: this.Longitude);
        }

        /// <summary>
        /// Longitude of location
        /// </summary>
        [Column("longitude"), Indexed]
        public double Longitude
        {
            get => this.Location.MapLocation?.Longitude ?? 0.0;
            set => this.Location.MapLocation = new MapPoint(latitude: this.Latitude, longitude: value);
        }

        /// <summary>
        /// Location as JSON
        /// </summary>
        [Column("json")]
        public string Json
        {
            get => JsonConvert.SerializeObject(this.Location);
            set => this.Location = JsonConvert.DeserializeObject<Location>(value);
        }

        /// <summary>
        /// Creates an empty location entry; used when loading entry from database
        /// </summary>
        public FindLocationEntry()
        {
            this.Location = new Location();
        }

        /// <summary>
        /// Creates a new entry from given location
        /// </summary>
        /// <param name="location">location to use</param>
        public FindLocationEntry(Location location)
        {
            this.Location = location;
        }
    }
}
