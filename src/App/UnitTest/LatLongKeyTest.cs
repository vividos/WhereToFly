using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WhereToFly.App.Serializers;
using WhereToFly.App.Services;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for record <see cref="LatLongKey"/>
    /// </summary>
    [TestClass]
    public class LatLongKeyTest
    {
        /// <summary>
        /// Tests serializing LatLongKey values
        /// </summary>
        [TestMethod]
        public void TestSerialization()
        {
            // set up
            var key = new LatLongKey(42, -64);
            var list = new List<LatLongKey>
            {
                new LatLongKey(42, -64),
                new LatLongKey(-128, 1),
            };

            var dict = new Dictionary<LatLongKey, List<Location>>
            {
                {
                    new LatLongKey(42, -64),
                    new List<Location> { new Location("42", new MapPoint(47.6764385, 11.8710533, 1685.0)) }
                },
            };

            // run
            string jsonKey = JsonSerializer.Serialize(
                key,
                ModelsJsonSerializerContext.Default.LatLongKey);

            string jsonList = JsonSerializer.Serialize(list);

            string jsonDict = JsonSerializer.Serialize(
                dict,
                ModelsJsonSerializerContext.Default.DictionaryLatLongKeyListLocation);

            LatLongKey key2 = JsonSerializer.Deserialize(
                jsonKey,
                ModelsJsonSerializerContext.Default.LatLongKey);

            List<LatLongKey>? list2 =
                JsonSerializer.Deserialize<List<LatLongKey>>(jsonList);

            Dictionary<LatLongKey, List<Location>>? dict2 = JsonSerializer.Deserialize(
                jsonDict,
                ModelsJsonSerializerContext.Default.DictionaryLatLongKeyListLocation);

            // check
            Assert.IsNotNull(list2, "deserialized list must not be null");
            Assert.IsNotNull(dict2, "deserialized dictionary must not be null");

            Assert.AreEqual(key, key2, "keys must be equal");
            Assert.IsTrue(list.SequenceEqual(list2), "lists must be equal");
            Assert.IsTrue(dict.Keys.SequenceEqual(dict2.Keys), "dictionaries must be equal");
        }
    }
}
