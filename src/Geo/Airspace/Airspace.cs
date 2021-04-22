namespace WhereToFly.Geo.Airspace
{
    /// <summary>
    /// Airspace containing data for one airspace definition
    /// </summary>
    public class Airspace
    {
        /// <summary>
        /// Airspace class
        /// </summary>
        public AirspaceClass Class { get; set; }

        /// <summary>
        /// Airspace name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Airspace description; collected from comment blocks right before an airspace; may be
        /// empty. Lines are split by newline characters.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Airspace floor altitude
        /// </summary>
        public Altitude Floor { get; set; }

        /// <summary>
        /// Airspace ceiling altitude
        /// </summary>
        public Altitude Ceiling { get; set; }

        /// <summary>
        /// Color of the airspace, as hex values, in the format: RRGGBB; may be null
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Airspace type; may be null. Naviter extension.
        /// </summary>
        public string AirspaceType { get; set; }

        /// <summary>
        /// Frequency of the controlling ATC station or other authority in that particular
        /// airspace; may be null. Naviter extension.
        /// </summary>
        public string Frequency { get; set; }

        /// <summary>
        /// Call sign for the ATC station; may be null. Naviter extension.
        /// </summary>
        public string CallSign { get; set; }

        /// <summary>
        /// Geometry describing the airspace
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// Creates new airspace object
        /// </summary>
        /// <param name="airspaceClass">airspace class</param>
        public Airspace(AirspaceClass airspaceClass)
        {
            this.Class = airspaceClass;
        }

        /// <summary>
        /// Formats a displayable text for the airspace
        /// </summary>
        /// <returns>airspace as text</returns>
        public override string ToString()
            => $"{this.Name} ({this.Class}) {this.Floor} -> {this.Ceiling}";
    }
}
