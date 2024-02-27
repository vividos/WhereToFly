using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using WhereToFly.App.Services;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for record LatLongKey
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
                new LatLongKey(-128, 1)
            };

            var dict = new Dictionary<LatLongKey, List<Location>>
            {
                {
                    new LatLongKey(42, -64),
                    new List<Location> { new Location("42", new MapPoint(47.6764385, 11.8710533, 1685.0)) }
                },
            };

            // run
            string jsonKey = JsonConvert.SerializeObject(key);
            string jsonList = JsonConvert.SerializeObject(list);
            string jsonDict = JsonConvert.SerializeObject(dict);

            var key2 = JsonConvert.DeserializeObject<LatLongKey>(jsonKey);
            var list2 = JsonConvert.DeserializeObject<List<LatLongKey>>(jsonList);
            var dict2 = JsonConvert.DeserializeObject<Dictionary<LatLongKey, List<Location>>>(jsonDict);

            // check
            Assert.AreEqual(key, key2, "keys must be equal");
            Assert.IsTrue(list.SequenceEqual(list2), "lists must be equal");
            Assert.IsTrue(dict.Keys.SequenceEqual(dict2?.Keys), "dictionaries must be equal");
        }
    }
}
