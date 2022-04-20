using SharpKml.Dom;
using SharpKml.Engine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.Geo.DataFormats;

namespace WhereToFly.Geo
{
    /// <summary>
    /// Importer for locations in a KML file; the importer makes sure that every inported location
    /// has a unique ID that also is stable between imports; this is done by deriving a unique ID
    /// based on the path of the Placemark objects in the KML file.
    /// </summary>
    public class KmlLocationImporter
    {
        /// <summary>
        /// Prefix for every ID
        /// </summary>
        private readonly string idPrefix;

        /// <summary>
        /// KML file to import
        /// </summary>
        private readonly KmlFile kmlFile;

        /// <summary>
        /// Set with all location IDs used so far
        /// </summary>
        private readonly HashSet<string> allLocationIds = new();

        /// <summary>
        /// Creates a new KML location importer from given stream
        /// </summary>
        /// <param name="idPrefix">prefix for IDs</param>
        /// <param name="stream">stream to read as KML/KMZ file</param>
        /// <param name="isKml">true when it's a .kml file, or false when it's a .kmz file</param>
        public KmlLocationImporter(string idPrefix, Stream stream, bool isKml)
        {
            this.idPrefix = idPrefix;

            if (isKml)
            {
                this.kmlFile = KmlFile.Load(stream);
            }
            else
            {
                var kmz = KmzFile.Open(stream);
                this.kmlFile = kmz.GetDefaultKmlFile();
            }
        }

        /// <summary>
        /// Imports all locations and returns a list of locations
        /// </summary>
        /// <returns>list of locations</returns>
        public Task<IEnumerable<Model.Location>> ImportAsync()
        {
            return Task.FromResult(
                this.GetLocationRecursive(this.idPrefix, this.kmlFile.Root));
        }

        /// <summary>
        /// Recursively goes through the KML and its Element objects and returns a list of
        /// locations found in the elements and sub-elements.
        /// </summary>
        /// <param name="currentIdPrefix">current prefix for ID</param>
        /// <param name="element">KML element to use</param>
        /// <returns>list of locations</returns>
        private IEnumerable<Model.Location> GetLocationRecursive(string currentIdPrefix, Element element)
        {
            switch (element)
            {
                case Kml kml:
                    return this.GetLocationRecursive(currentIdPrefix, kml.Feature);

                case Document doc:
                    return doc.Features.SelectMany(feature => this.GetLocationRecursive(currentIdPrefix, feature));

                case Folder folder:
                    string subIdPrefix = currentIdPrefix + "-" + folder.Name;
                    return folder.Features.SelectMany(feature => this.GetLocationRecursive(subIdPrefix, feature));

                case Placemark placemark when placemark.Geometry is Point:
                    string locationId = SanitizeId(
                        currentIdPrefix +
                        (!string.IsNullOrEmpty(placemark.Id)
                        ? placemark.Id
                        : placemark.Name.Replace(" ", "-").Normalize()));

                    if (!this.allLocationIds.Contains(locationId))
                    {
                        Model.Location location = KmlDataFile.LocationFromPlacemark(this.kmlFile, placemark);
                        location.Id = locationId;

                        this.allLocationIds.Add(locationId);

                        return new[] { location };
                    }
                    else
                    {
                        return Enumerable.Empty<Model.Location>();
                    }

                default:
                    return Enumerable.Empty<Model.Location>();
            }
        }

        /// <summary>
        /// Sanitizes an ID by removing anything which is not allowed
        /// </summary>
        /// <param name="id">ID to sanitize</param>
        /// <returns>sanitized ID</returns>
        private static string SanitizeId(string id)
        {
            return id
                .Replace("<b>", string.Empty)
                .Replace("</b>", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace(" ", string.Empty)
                .Replace(",", "-")
                .Replace("/", "-")
                .Replace("--", "-");
        }
    }
}
