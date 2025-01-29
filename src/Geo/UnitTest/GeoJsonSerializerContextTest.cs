using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using WhereToFly.Geo.DataFormats.GeoJson;

namespace WhereToFly.Geo.UnitTest
{
    /// <summary>
    /// Unit tests for GeoJSON JSON serializer context
    /// </summary>
    [TestClass]
    public class GeoJsonSerializerContextTest
    {
        /// <summary>
        /// Tests deserializing polymorphic JSON, geometry with PointGeometry.
        /// </summary>
        [TestMethod]
        public void TestConvertPolymorphicGeometry_Point()
        {
            // set up
            string pointGeometryJson = @"
{
    ""type"": ""Point"",
    ""coordinates"": [102.0, 0.5]
}
";

            // run
            var pointGeometry = JsonSerializer.Deserialize<Geometry>(
                pointGeometryJson,
                GeoJsonSerializerContext.Default.Geometry);

            // check
            Assert.AreEqual(
                "PointGeometry",
                pointGeometry?.GetType().Name,
                "geometry must be a point geometry");
        }

        /// <summary>
        /// Tests deserializing polymorphic JSON, Feature with PointGeometry.
        /// </summary>
        [TestMethod]
        public void TestConvertPolymorphicFeature_Geometry_Point()
        {
            // set up
            string featureWithPointGeometryJson = @"
{
    ""type"": ""Feature"",
    ""geometry"": {
    ""type"": ""Point"",
    ""coordinates"": [102.0, 0.5]
    },
    ""properties"": {
    ""prop0"": ""value0""
    }
}
";

            // run
            var featureWithPointGeometry =
                JsonSerializer.Deserialize<Element>(
                    featureWithPointGeometryJson,
                    GeoJsonSerializerContext.Default.Element);

            // check
            Assert.IsTrue(
                featureWithPointGeometry is Feature,
                "element must be a Feature");

            Assert.IsTrue(
                (featureWithPointGeometry as Feature)?.Geometry is PointGeometry,
                "geometry must be a point geometry");
        }
    }
}
