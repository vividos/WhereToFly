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
        /// Element style ID for all KML elements
        /// </summary>
        private const string KmlElementStyleId = "element_style";

        /// <summary>
        /// KML conversion options
        /// </summary>
        private readonly KmlConvertOptions kmlOptions;

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

            this.AddElementStyle(kml);

            if (rootElement != null)
            {
                this.ConvertElementToKml(folder, this.kmlOptions.DocumentName, rootElement);
            }

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
        private void AddElementStyle(Kml kml)
        {
            var style = new Style
            {
                Id = KmlElementStyleId,
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

                        if (featureCollection.FeatureList != null)
                        {
                            foreach (Feature feature in featureCollection.FeatureList)
                            {
                                this.ConvertElementToKml(subFolder, elementName, feature);
                            }
                        }
                        else
                        {
                            throw new FormatException("GeoJSON FeatureCollection without feature list");
                        }

                        break;
                    }

                case Feature feature:
                    if (feature.Geometry != null)
                    {
                        this.ConvertGeometryToKml(folder, elementName, feature.Geometry);
                    }
                    else
                    {
                        throw new FormatException("GeoJSON Feature without geometry");
                    }

                    break;

                case GeometryCollection geometryCollection:
                    if (geometryCollection.GeometryList != null)
                    {
                        foreach (var singleGeometry in geometryCollection.GeometryList)
                        {
                            this.ConvertGeometryToKml(folder, elementName, singleGeometry);
                        }
                    }
                    else
                    {
                        throw new FormatException("GeoJSON GeometryCollection without geometry list");
                    }

                    break;

                default:
                    throw new FormatException("invalid element type");
            }
        }

        /// <summary>
        /// Property keys for potential name of an element
        /// </summary>
        private static readonly string[] NamePropertyKeys =
            ["name", "NAME", "title", "TITLE", "OBJECTID"];

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
                return element.Title!;
            }

            if (element.Properties != null)
            {
                foreach (string key in NamePropertyKeys)
                {
                    if (element.Properties.TryGetValue(key, out object? value))
                    {
                        return value?.ToString() ?? string.Empty;
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
                    if (pointGeometry.Coordinates != null)
                    {
                        this.AddPointPlacemark(
                            folder,
                            elementName,
                            pointGeometry.Coordinates);
                    }
                    else
                    {
                        throw new FormatException("GeoJSON PointGeometry without coordinates");
                    }

                    break;

                case MultiPointGeometry multiPointGeometry:
                    if (multiPointGeometry.Coordinates != null)
                    {
                        foreach (double[] pointCollection in multiPointGeometry.Coordinates)
                        {
                            this.AddPointPlacemark(
                                folder,
                                elementName,
                                pointCollection);
                        }
                    }
                    else
                    {
                        throw new FormatException("GeoJSON MultiPointGeometry without coordinates");
                    }

                    break;

                case LineStringGeometry lineStringGeometry:
                    if (lineStringGeometry.Coordinates != null)
                    {
                        this.AddLineStringPlacemark(
                            folder,
                            elementName,
                            lineStringGeometry.Coordinates);
                    }
                    else
                    {
                        throw new FormatException("GeoJSON LineStringGeometry without coordinates");
                    }

                    break;

                case MultiLineStringGeometry multiLineStringGeometry:
                    if (multiLineStringGeometry.Coordinates != null)
                    {
                        foreach (double[][] lineStringCollection in multiLineStringGeometry.Coordinates)
                        {
                            this.AddLineStringPlacemark(folder, elementName, lineStringCollection);
                        }
                    }
                    else
                    {
                        throw new FormatException("GeoJSON MultiLineStringGeometry without coordinates");
                    }

                    break;

                case PolygonGeometry polygonGeometry:
                    if (polygonGeometry.Coordinates != null)
                    {
                        foreach (double[][] polygonCollection in polygonGeometry.Coordinates)
                        {
                            this.AddPolygonPlacemark(folder, elementName, polygonCollection);
                        }
                    }
                    else
                    {
                        throw new FormatException("GeoJSON PolygonGeometry without coordinates");
                    }

                    break;

                case MultiPolygonGeometry multiPolygonGeometry:
                    if (multiPolygonGeometry.Coordinates != null)
                    {
                        foreach (double[][][] multiPolygonCollection in multiPolygonGeometry.Coordinates)
                        {
                            foreach (double[][] polygonCollection in multiPolygonCollection)
                            {
                                this.AddPolygonPlacemark(folder, elementName, polygonCollection);
                            }
                        }
                    }
                    else
                    {
                        throw new FormatException("GeoJSON MultiPolygonGeometry without coordinates");
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
                StyleUrl = new Uri($"#{KmlElementStyleId}", UriKind.Relative),
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
                Coordinates = [],
            };

            foreach (double[] coordinates in coordinatesList)
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
                StyleUrl = new Uri($"#{KmlElementStyleId}", UriKind.Relative),
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
                        Coordinates = [],
                    },
                },
            };

            foreach (double[] coordinates in polygonCollection)
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
                StyleUrl = new Uri($"#{KmlElementStyleId}", UriKind.Relative),
            };

            folder.AddFeature(placemark);
        }
        #endregion
    }
}
