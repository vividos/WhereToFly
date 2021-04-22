using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.Geo.DataFormats.GeoJson;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Unit tests for GeoJSON converter classes
    /// </summary>
    [TestClass]
    public class GeoJsonConvertTest
    {
        /// <summary>
        /// Example GeoJSON strings, from Wikipedia
        /// </summary>
        private static readonly string[] GeoJsonExamples = new string[]
        {
            @"{
  ""type"": ""FeatureCollection"",
  ""features"": [
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [102.0, 0.5]
      },
      ""properties"": {
        ""prop0"": ""value0""
      }
    },
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""LineString"",
        ""coordinates"": [
          [102.0, 0.0], [103.0, 1.0], [104.0, 0.0], [105.0, 1.0]
        ]
      },
      ""properties"": {
        ""prop0"": ""value0"",
        ""prop1"": 0.0
      }
    },
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""Polygon"",
        ""coordinates"": [
          [
            [100.0, 0.0], [101.0, 0.0], [101.0, 1.0],
            [100.0, 1.0], [100.0, 0.0]
          ]
        ]
      },
      ""properties"": {
        ""prop0"": ""value0"",
        ""prop1"": { ""this"": ""that"" }
      }
    }
  ]
}",
            @"{
    ""type"": ""Point"",
    ""coordinates"": [30, 10]
}",
            @"{
    ""type"": ""LineString"",
    ""coordinates"": [
        [30, 10], [10, 30], [40, 40]
    ]
}",
            @"{
    ""type"": ""Polygon"",
    ""coordinates"": [
        [[30, 10], [40, 40], [20, 40], [10, 20], [30, 10]]
    ]
}",
            @"{
    ""type"": ""MultiPoint"",
    ""coordinates"": [
        [10, 40], [40, 30], [20, 20], [30, 10]
    ]
}",
            @"{
    ""type"": ""MultiLineString"",
    ""coordinates"": [
        [[10, 10], [20, 20], [10, 40]],
        [[40, 40], [30, 30], [40, 20], [30, 10]]
    ]
}",
            @"{
    ""type"": ""MultiPolygon"",
    ""coordinates"": [
        [
            [[30, 20], [45, 40], [10, 40], [30, 20]]
        ],
        [
            [[15, 5], [40, 10], [10, 20], [5, 10], [15, 5]]
        ]
    ]
}",
            @"{
    ""type"": ""GeometryCollection"",
    ""geometries"": [
        {
            ""type"": ""Point"",
            ""coordinates"": [40, 10]
        },
        {
    ""type"": ""LineString"",
            ""coordinates"": [
                [10, 10], [20, 20], [10, 40]
            ]
        },
        {
    ""type"": ""Polygon"",
            ""coordinates"": [
                [[40, 40], [20, 45], [45, 30], [40, 40]]
            ]
        }
    ]
}",
        };

        /// <summary>
        /// Tests converting GeoJSON to KML format
        /// </summary>
        [TestMethod]
        public void TestConvertToKml()
        {
            foreach (var geoJsonText in GeoJsonExamples)
            {
                try
                {
                    // set up
                    var kmlOptions = new KmlConvertOptions
                    {
                        DocumentName = "empty",
                        PolygonColor = SharpKml.Base.Color32.Parse("aabb12"),
                    };

                    var converter = new GeoJsonKmlConverter(kmlOptions);

                    // run
                    string kml = converter.ConvertToKml(geoJsonText);

                    // check
                    Assert.IsTrue(!string.IsNullOrEmpty(kml), "generated KML must not be empty");
                }
                catch (Exception ex)
                {
                    Assert.Fail("generating KML must not throw an exception: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Tests converting GeoJSON to CZML format
        /// </summary>
        [TestMethod]
        public void TestConvertToCzml()
        {
            foreach (var geoJsonText in GeoJsonExamples)
            {
                try
                {
                    // set up
                    var czmlConvertOptions = new CzmlConvertOptions
                    {
                        DocumentName = "empty",
                        DocumentDescription = "also empty",
                        PointSize = 15.0,
                        PointColor = new WhereToFly.Geo.DataFormats.Czml.Color(255, 0, 255),
                        LineWidth = 10.0,
                        LineColor = new WhereToFly.Geo.DataFormats.Czml.Color(0, 255, 255),
                        PolygonColor = new WhereToFly.Geo.DataFormats.Czml.Color(255, 255, 0),
                        CustomNameFormatter = (element) => element.Title ?? "title",
                        CustomDescriptionFormatter = (element) => element.Title ?? "description",
                        CustomPointColorResolver = (element) =>
                            new WhereToFly.Geo.DataFormats.Czml.Color(element.GetHashCode() & 0xFF, 0, 0),
                    };

                    // run
                    var converter = new GeoJsonCzmlConverter(czmlConvertOptions);
                    string czml = converter.ConvertToCzml(geoJsonText);

                    // check
                    Assert.IsTrue(!string.IsNullOrEmpty(czml), "generated CZML must not be empty");
                }
                catch (Exception ex)
                {
                    Assert.Fail("generating CZML must not throw an exception: " + ex.ToString());
                }
            }
        }
    }
}
