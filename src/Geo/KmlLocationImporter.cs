using SharpKml.Dom;
using SharpKml.Engine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.Geo.DataFormats;
using WhereToFly.Geo.Model;

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
        private readonly HashSet<string> allLocationIds = [];

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
                this.GetLocationRecursive(this.kmlFile.Root));
        }

        /// <summary>
        /// Recursively goes through the KML and its Element objects and returns a list of
        /// locations found in the elements and sub-elements.
        /// </summary>
        /// <param name="element">KML element to use</param>
        /// <returns>list of locations</returns>
        private IEnumerable<Model.Location> GetLocationRecursive(Element element)
        {
            switch (element)
            {
                case Kml kml:
                    return this.GetLocationRecursive(kml.Feature);

                case Document doc:
                    return doc.Features.SelectMany(this.GetLocationRecursive);

                case Folder folder:
                    return folder.Features.SelectMany(this.GetLocationRecursive);

                case Placemark placemark when placemark.Geometry is Point:
                    Model.Location location = KmlDataFile.LocationFromPlacemark(
                        this.kmlFile,
                        placemark,
                        true,
                        this.idPrefix);

                    if (!this.allLocationIds.Contains(location.Id))
                    {
                        this.allLocationIds.Add(location.Id);

                        return [location];
                    }
                    else
                    {
                        return [];
                    }

                default:
                    return [];
            }
        }
    }
}
