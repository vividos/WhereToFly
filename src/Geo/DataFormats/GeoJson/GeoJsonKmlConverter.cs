using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WhereToFly.Geo.DataFormats.GeoJson
{
    /// <summary>
    /// Converter to convert GeoJSON to KML format. The GeoJSON elements are translated to the
    /// appropriate KML elements.
    /// </summary>
    public class GeoJsonKmlConverter
    {
        /// <summary>
        /// KML conversion options
        /// </summary>
        private readonly KmlConvertOptions kmlOptions;

        /// <summary>
        /// Element style, to be used for all KML elements
        /// </summary>
        private Style elementStyle;

        /// <summary>
        /// Creates a new GeoJSON to KML converter
        /// </summary>
        /// <param name="kmlOptions">KML conversion options</param>
        public GeoJsonKmlConverter(KmlConvertOptions kmlOptions)
        {
            this.kmlOptions = kmlOptions;
        }

        /// <summary>
        /// Converts given GeoJSON text to a KML document
        /// </summary>
        /// <param name="geoJsonText">GeoJSON text</param>
        /// <returns>KML text</returns>
        public string ConvertToKml(string geoJsonText)
        {
            var rootElement = Element.Deserialize(geoJsonText);

            var kml = new Kml();

            var folder = new Folder
            {
                Name = this.kmlOptions.DocumentName,
            };

            kml.Feature = folder;

            this.elementStyle = this.AddElementStyle(kml);

            this.ConvertElementToKml(folder, this.kmlOptions.DocumentName, rootElement);

            var kmlfile = KmlFile.Create(kml, false);
            using var stream = new MemoryStream();
            kmlfile.Save(stream);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        #region KML conversion implementation
        /// <summary>
        /// Adds a Style object to the KML object, to be used for all elements
        /// </summary>
        /// <param name="kml">KML object to add to</param>
        /// <returns>KML Style object</returns>
        private Style AddElementStyle(Kml kml)
        {
            var style = new Style
            {
                Id = "element_style",
                Line = new LineStyle
                {
                    Width = this.kmlOptions.LineWidth,
                },
                Polygon = new PolygonStyle
                {
                    Fill = true,
                    Outline = true,
                    Color = this.kmlOptions.PolygonColor,
                },
            };

            kml.Feature.AddStyle(style);

            return style;
        }

        /// <summary>
        /// Converts a GeoJSON element to a KML element and adds it to the KML folder.
        /// </summary>
        /// <param name="folder">KML folder</param>
        /// <param name="parentElementName">the parent's element name</param>
        /// <param name="element">GeoJSON element</param>
        private void ConvertElementToKml(
            Folder folder,
            string parentElementName,
            Element element)
        {
            string elementName = this.NameFromElement(element, parentElementName);

            switch (element)
            {
                case FeatureCollection featureCollection:
                    {
                        var subFolder = new Folder
                        {
                            Name = elementName,
                        };

                        folder.AddFeature(subFolder);

                        foreach (Feature feature in featureCollection.FeatureList)
                        {
                            this.ConvertElementToKml(subFolder, elementName, feature);
                        }

                        break;
                    }

                case Feature feature:
                    this.ConvertElementToKml(folder, elementName, feature.Geometry);
                    break;

                case Geometry geometry:
                    this.ConvertGeometryToKml(folder, elementName, geometry);
                    break;

                case GeometryCollection geometryCollection:
                    {
                        foreach (var singleGeometry in geometryCollection.GeometryList)
                        {
                            this.ConvertGeometryToKml(folder, elementName, singleGeometry);
                        }

                        break;
                    }

                default:
                    throw new FormatException("invalid element type");
            }
        }

        /// <summary>
        /// Property keys for potential name of an element
        /// </summary>
        private static readonly string[] NamePropertyKeys =
            new[] { "name", "NAME", "title", "TITLE", "OBJECTID" };

        /// <summary>
        /// Generates a name from a given element
        /// </summary>
        /// <param name="element">element object</param>
        /// <param name="defaultName">default name if none could be found</param>
        /// <returns>element name</returns>
        private string NameFromElement(Element element, string defaultName)
        {
            if (this.kmlOptions.CustomNameFormatter != null)
            {
                string customName =
                    this.kmlOptions.CustomNameFormatter.Invoke(element.Properties);

                if (!string.IsNullOrWhiteSpace(customName))
                {
                    return customName;
                }
            }

            if (!string.IsNullOrWhiteSpace(element.Title))
            {
                return element.Title;
            }

            if (element.Properties != null)
            {
                foreach (string key in NamePropertyKeys)
                {
                    if (element.Properties.ContainsKey(key))
                    {
                        return element.Properties[key].ToString();
                    }
                }
            }

            return defaultName;
        }

        /// <summary>
        /// Converts a GeoJSON geometry to a KML element and adds it to the KML folder.
        /// </summary>
        /// <param name="folder">KML folder</param>
        /// <param name="elementName">name of element to create</param>
        /// <param name="geometry">GeoJSON geometry</param>
        private void ConvertGeometryToKml(
            Folder folder,
            string elementName,
            Geometry geometry)
        {
            switch (geometry)
            {
                case PointGeometry pointGeometry:
                    this.AddPointPlacemark(
                        folder,
                        elementName,
                        pointGeometry.Coordinates);
                    break;

                case MultiPointGeometry multiPointGeometry:
                    foreach (var pointCollection in multiPointGeometry.Coordinates)
                    {
                        this.AddPointPlacemark(
                            folder,
                            elementName,
                            pointCollection);
                    }

                    break;

                case LineStringGeometry lineStringGeometry:
                    this.AddLineStringPlacemark(
                        folder,
                        elementName,
                        lineStringGeometry.Coordinates);
                    break;

                case MultiLineStringGeometry multiLineStringGeometry:
                    foreach (var lineStringCollection in multiLineStringGeometry.Coordinates)
                    {
                        this.AddLineStringPlacemark(folder, elementName, lineStringCollection);
                    }

                    break;

                case PolygonGeometry polygonGeometry:
                    foreach (var polygonCollection in polygonGeometry.Coordinates)
                    {
                        this.AddPolygonPlacemark(folder, elementName, polygonCollection);
                    }

                    break;

                case MultiPolygonGeometry multiPolygonGeometry:
                    foreach (var multiPolygonCollection in multiPolygonGeometry.Coordinates)
                    {
                        foreach (var polygonCollection in multiPolygonCollection)
                        {
                            this.AddPolygonPlacemark(folder, elementName, polygonCollection);
                        }
                    }

                    break;

                default:
                    throw new FormatException("invalid geometry type");
            }
        }

        /// <summary>
        /// Adds a single point placemark with given coordinates
        /// </summary>
        /// <param name="folder">folder to add to</param>
        /// <param name="elementName">name of element to create</param>
        /// <param name="coordinates">coordinates for the point</param>
        private void AddPointPlacemark(
            Folder folder,
            string elementName,
            double[] coordinates)
        {
            Debug.Assert(
                coordinates.Length == 2,
                "there always must be 2 coordinate values");

            var point = new Point
            {
                AltitudeMode = AltitudeMode.ClampToGround,
                Extrude = false,
                Coordinate = new Vector(
                    latitude: coordinates[1],
                    longitude: coordinates[0]),
            };

            var placemark = new Placemark
            {
                Name = elementName,
                Geometry = point,
                StyleUrl = new Uri($"#{this.elementStyle.Id}", UriKind.Relative),
            };

            folder.AddFeature(placemark);
        }

        /// <summary>
        /// Adds a single line string placemark with given coordinates
        /// </summary>
        /// <param name="folder">folder to add to</param>
        /// <param name="elementName">name of element to create</param>
        /// <param name="coordinatesList">list of coordinates</param>
        private void AddLineStringPlacemark(
            Folder folder,
            string elementName,
            double[][] coordinatesList)
        {
            var lineString = new LineString
            {
                AltitudeMode = AltitudeMode.ClampToGround,
                Extrude = false,
                Coordinates = new CoordinateCollection(),
            };

            foreach (var coordinates in coordinatesList)
            {
                Debug.Assert(
                    coordinates.Length == 2,
                    "there always must be 2 coordinate values");

                lineString.Coordinates.Add(
                    new Vector(
                        latitude: coordinates[1],
                        longitude: coordinates[0]));
            }

            var placemark = new Placemark
            {
                Name = elementName,
                Geometry = lineString,
                StyleUrl = new Uri($"#{this.elementStyle.Id}", UriKind.Relative),
            };

            folder.AddFeature(placemark);
        }

        /// <summary>
        /// Adds a single polygon placemark with given polygon point coordinates
        /// </summary>
        /// <param name="folder">folder to add to</param>
        /// <param name="elementName">name of element to create</param>
        /// <param name="polygonCollection">polygon point coordinates</param>
        private void AddPolygonPlacemark(
            Folder folder,
            string elementName,
            double[][] polygonCollection)
        {
            var polygon = new Polygon
            {
                AltitudeMode = AltitudeMode.ClampToGround,
                Extrude = false,
                OuterBoundary = new OuterBoundary
                {
                    LinearRing = new LinearRing
                    {
                        Coordinates = new CoordinateCollection(),
                    },
                },
            };

            foreach (var coordinates in polygonCollection)
            {
                Debug.Assert(
                    coordinates.Length == 2,
                    "there always must be 2 coordinate values");

                polygon.OuterBoundary.LinearRing.Coordinates.Add(
                    new Vector(
                        latitude: coordinates[1],
                        longitude: coordinates[0]));
            }

            var placemark = new Placemark
            {
                Name = elementName,
                Geometry = polygon,
                StyleUrl = new Uri($"#{this.elementStyle.Id}", UriKind.Relative),
            };

            folder.AddFeature(placemark);
        }
        #endregion
    }
}
