using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WhereToFly.Geo.DataFormats.GeoJson
{
    /// <summary>
    /// Converter to convert GeoJSON to CZML format. The elements are translated to the
    /// appropriate CZML elements.
    /// </summary>
    public class GeoJsonCzmlConverter
    {
        /// <summary>
        /// CZML conversion options
        /// </summary>
        private readonly CzmlConvertOptions czmlOptions;

        /// <summary>
        /// Creates a new GeoJSON to CZML converter
        /// </summary>
        /// <param name="czmlOptions">CZML conversion options</param>
        public GeoJsonCzmlConverter(CzmlConvertOptions czmlOptions)
        {
            this.czmlOptions = czmlOptions;
        }

        /// <summary>
        /// Converts given GeoJSON text to a CZML document
        /// </summary>
        /// <param name="geoJsonText">GeoJSON text</param>
        /// <returns>CZML JSON data</returns>
        public string ConvertToCzml(string geoJsonText)
        {
            var rootElement = Element.Deserialize(geoJsonText);

            var objectList = new List<object>
            {
                new Czml.PacketHeader(
                    this.czmlOptions.DocumentName,
                    this.czmlOptions.DocumentDescription),
            };

            this.ConvertElementToCzml(rootElement, objectList);

            return JsonConvert.SerializeObject(objectList);
        }

        #region CZML conversion implementation
        /// <summary>
        /// Converts a GeoJSON element to a CZML element and adds it to the object list.
        /// </summary>
        /// <param name="element">GeoJSON element</param>
        /// <param name="objectList">object list to add to</param>
        private void ConvertElementToCzml(Element element, List<object> objectList)
        {
            switch (element)
            {
                case FeatureCollection featureCollection:
                    foreach (Feature feature in featureCollection.FeatureList)
                    {
                        this.ConvertElementToCzml(feature, objectList);
                    }

                    break;

                case Feature feature:
                    this.ConvertGeometryToCzml(feature, feature.Geometry, objectList);
                    break;

                case Geometry geometry:
                    this.ConvertGeometryToCzml(element, geometry, objectList);
                    break;

                case GeometryCollection geometryCollection:
                    {
                        foreach (var singleGeometry in geometryCollection.GeometryList)
                        {
                            this.ConvertGeometryToCzml(element, singleGeometry, objectList);
                        }

                        break;
                    }

                default:
                    throw new FormatException("invalid element type");
            }
        }

        /// <summary>
        /// Converts a GeoJSON geometry to a CZML element and adds it to the object list.
        /// </summary>
        /// <param name="element">GeoJSON element containing geometry</param>
        /// <param name="geometry">GeoJSON geometry</param>
        /// <param name="objectList">object list to add to</param>
        private void ConvertGeometryToCzml(
            Element element,
            Geometry geometry,
            List<object> objectList)
        {
            string elementName = this.NameFromElement(element, "???");
            string elementDescription = this.DescriptionFromElement(element);

            switch (geometry)
            {
                case PointGeometry pointGeometry:
                    objectList.Add(
                        this.GetPointObject(
                            element,
                            elementName,
                            elementDescription,
                            pointGeometry.Coordinates));
                    break;

                case MultiPointGeometry multiPointGeometry:
                    foreach (var pointCollection in multiPointGeometry.Coordinates)
                    {
                        objectList.Add(
                            this.GetPointObject(
                                element,
                                elementName,
                                elementDescription,
                                pointCollection));
                    }

                    break;

                case LineStringGeometry lineStringGeometry:
                    objectList.Add(
                        this.GetLineStringPolylineObject(
                            elementName,
                            elementDescription,
                            lineStringGeometry.Coordinates));
                    break;

                case MultiLineStringGeometry multiLineStringGeometry:
                    foreach (var lineStringCollection in multiLineStringGeometry.Coordinates)
                    {
                        objectList.Add(
                            this.GetLineStringPolylineObject(
                                elementName,
                                elementDescription,
                                lineStringCollection));
                    }

                    break;

                case PolygonGeometry polygonGeometry:
                    foreach (var polygonCollection in polygonGeometry.Coordinates)
                    {
                        objectList.Add(
                            this.GetPolygonPlacemark(
                                elementName,
                                elementDescription,
                                polygonCollection));
                    }

                    break;

                case MultiPolygonGeometry multiPolygonGeometry:
                    foreach (var multiPolygonCollection in multiPolygonGeometry.Coordinates)
                    {
                        foreach (var polygonCollection in multiPolygonCollection)
                        {
                            objectList.Add(
                                this.GetPolygonPlacemark(
                                    elementName,
                                    elementDescription,
                                    polygonCollection));
                        }
                    }

                    break;

                default:
                    throw new FormatException("invalid geometry type");
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
            if (this.czmlOptions.CustomNameFormatter != null)
            {
                string customName = this.czmlOptions.CustomNameFormatter.Invoke(element);
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
        /// Property keys for potential description of an element
        /// </summary>
        private static readonly string[] DescriptionPropertyKeys =
            new[] { "desc", "DESC", "description", "DESCRIPTION" };

        /// <summary>
        /// Generates a description from a given element
        /// </summary>
        /// <param name="element">element object</param>
        /// <returns>element description</returns>
        private string DescriptionFromElement(Element element)
        {
            if (this.czmlOptions.CustomDescriptionFormatter != null)
            {
                string customDescription =
                    this.czmlOptions.CustomDescriptionFormatter.Invoke(element);

                if (!string.IsNullOrWhiteSpace(customDescription))
                {
                    return customDescription;
                }
            }

            if (element.Properties != null)
            {
                foreach (string key in DescriptionPropertyKeys)
                {
                    if (element.Properties.ContainsKey(key))
                    {
                        return element.Properties[key].ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Creates a point CZML object from given GeoJSON point element
        /// </summary>
        /// <param name="element">GeoJSON element to convert</param>
        /// <param name="elementName">element name</param>
        /// <param name="elementDescription">element description</param>
        /// <param name="coordinates">coordinates array</param>
        /// <returns>CZML point object</returns>
        private Czml.Object GetPointObject(
            Element element,
            string elementName,
            string elementDescription,
            double[] coordinates)
        {
            Debug.Assert(
                coordinates.Length == 2 || coordinates.Length == 3,
                "there always must be 2 or 3 coordinate values");

            return new Czml.Object
            {
                Name = elementName,
                Description = elementDescription,
                Position = new Czml.PositionList(
                    latitude: coordinates[1],
                    longitude: coordinates[0],
                    height: coordinates.Length == 3 ? coordinates[2] : null),
                Point = new Czml.Point
                {
                    PixelSize = this.czmlOptions.PointSize,
                    Color = this.ColorForPoint(element),
                    HeightReference = Czml.HeightReference.ClampToGround,
                    OutlineWidth = 3.0,
                    OutlineColor = new Czml.Color(0, 0, 0),
                },
            };
        }

        /// <summary>
        /// Returns a CZML color object for given GeoJSON point element
        /// </summary>
        /// <param name="element">GeoJSON element to use</param>
        /// <returns>color object</returns>
        private Czml.Color ColorForPoint(Element element)
        {
            if (this.czmlOptions.CustomPointColorResolver != null)
            {
                Czml.Color color = this.czmlOptions.CustomPointColorResolver.Invoke(element);
                if (color != null)
                {
                    return color;
                }
            }

            return this.czmlOptions.PointColor;
        }

        /// <summary>
        /// Creates a CZML polyline from given GeoJSON line string properties
        /// </summary>
        /// <param name="elementName">element name</param>
        /// <param name="elementDescription">element description</param>
        /// <param name="coordinatesList">coordinates list</param>
        /// <returns>CZML polyline object</returns>
        private Czml.Object GetLineStringPolylineObject(
            string elementName,
            string elementDescription,
            double[][] coordinatesList)
        {
            var positionList = new Czml.PositionList();

            foreach (var coordinates in coordinatesList)
            {
                Debug.Assert(
                    coordinates.Length == 2 || coordinates.Length == 3,
                    "there always must be 2 or 3 coordinate values");

                positionList.Add(
                    latitude: coordinates[1],
                    longitude: coordinates[0],
                    height: coordinates.Length == 3 ? coordinates[2] : null);
            }

            return new Czml.Object
            {
                Name = elementName,
                Description = elementDescription,
                Polyline = new Czml.Polyline
                {
                    Width = this.czmlOptions.LineWidth,
                    Material = Czml.Material.FromSolidColor(this.czmlOptions.LineColor),
                    ClampToGround = true,
                    Positions = positionList,
                },
            };
        }

        /// <summary>
        /// Creates a CZML polygon from given GeoJSON polygon properties
        /// </summary>
        /// <param name="elementName">element name</param>
        /// <param name="elementDescription">element description</param>
        /// <param name="coordinatesList">coordinates list</param>
        /// <returns>CZML polygon object</returns>
        private Czml.Object GetPolygonPlacemark(
            string elementName,
            string elementDescription,
            double[][] coordinatesList)
        {
            var positionList = new Czml.PositionList();

            foreach (var coordinates in coordinatesList)
            {
                Debug.Assert(
                    coordinates.Length == 2,
                    "there always must be 2 coordinate values");

                positionList.Add(
                    latitude: coordinates[1],
                    longitude: coordinates[0],
                    height: null);
            }

            return new Czml.Object
            {
                Name = elementName,
                Description = elementDescription,
                Polygon = new Czml.Polygon
                {
                    Positions = positionList,
                    Material = Czml.Material.FromSolidColor(this.czmlOptions.PolygonColor),
                    Outline = false,
                    OutlineColor = new Czml.Color(0, 0, 0),
                },
            };
        }
        #endregion
    }
}
