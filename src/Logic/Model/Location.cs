namespace WhereToFly.Logic.Model
{
    /// <summary>
    /// A location that can be used for tour planning, e.g. as intermediate stops.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Location ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Elevation of location, in meter above sea level
        /// </summary>
        public double Elevation { get; set; }

        /// <summary>
        /// Location on map
        /// </summary>
        public MapPoint MapLocation { get; set; }

        /// <summary>
        /// Description of location
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of location
        /// </summary>
        public LocationType Type { get; set; }

        /// <summary>
        /// Link to external internet page, for more infos about location
        /// </summary>
        public string InternetLink { get; set; }

        #region object overridables implementation
        /// <summary>
        /// Calculates hash code for map point
        /// </summary>
        /// <returns>calculated hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = 487;

            hashCode = (hashCode * 31) + this.Id.GetHashCode();
            hashCode = (hashCode * 31) + this.Name.GetHashCode();
            hashCode = (hashCode * 31) + this.Type.GetHashCode();
            hashCode = (hashCode * 31) + this.InternetLink.GetHashCode();
            hashCode = (hashCode * 31) + this.MapLocation.GetHashCode();
            hashCode = (hashCode * 31) + this.Elevation.GetHashCode();
            hashCode = (hashCode * 31) + this.Description.GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Returns a printable representation of this object
        /// </summary>
        /// <returns>printable text</returns>
        public override string ToString()
        {
            return $"Name={this.Name}, Type={this.Type}, MapLocation={this.MapLocation}, Elevation={this.Elevation}";
        }
        #endregion
    }
}
