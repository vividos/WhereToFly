using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WhereToFly.App.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Test for Location class
    /// </summary>
    [TestClass]
    public class LocationTest
    {
        /// <summary>
        /// Returns default location for tests
        /// </summary>
        /// <returns>default location</returns>
        private static Location GetDefaultLocation()
        {
            return new Location
            {
                Id = Guid.NewGuid().ToString("B"),
                Name = "Brecherspitz",
                Elevation = 1685,
                MapLocation = new MapPoint(47.6764385, 11.8710533),
                Description = "Herrliche Aussicht über die drei Seen Schliersee im Norden, Tegernsee im Westen und den Spitzingsee im Süden.",
                Type = LocationType.Summit,
                InternetLink = "https://de.wikipedia.org/wiki/Brecherspitz"
            };
        }

        /// <summary>
        /// Tests ToString() method
        /// </summary>
        [TestMethod]
        public void TestToString()
        {
            // set up
            var location = GetDefaultLocation();

            // run
            string text = location.ToString();

            // check
            Assert.IsTrue(text.Length > 0, "location ToString() result must contain text");
        }

        /// <summary>
        /// Tests GetHashCode() method
        /// </summary>
        [TestMethod]
        public void TestGetHashCode()
        {
            // set up
            var location1 = GetDefaultLocation();
            var location2 = GetDefaultLocation();
            location2.Id = location1.Id;

            var location3 = GetDefaultLocation();

            // run + check
            var dict = new Dictionary<Location, bool>();
            Assert.IsFalse(dict.ContainsKey(location1), "location must not be in dictionary yet");

            dict.Add(location1, true);

            Assert.IsTrue(dict.ContainsKey(location1), "location must be in dictionary now");
            ////Assert.IsTrue(dict.ContainsKey(location2), "location in different reference must also be found in dict");
            Assert.IsFalse(dict.ContainsKey(location3), "different location must not be found in dict");

            dict.Remove(location1);
            Assert.IsFalse(dict.ContainsKey(location1), "location must not be in dictionary now");
        }

        /// <summary>
        /// Tests serializing and deserializing list of Location objects from/to JSON
        /// </summary>
        [TestMethod]
        public void TestJsonSerialisation()
        {
            // set up
            var locationList = new List<Location>
            {
                GetDefaultLocation()
            };

            // run
            string json = JsonConvert.SerializeObject(locationList);
            var locationList2 = JsonConvert.DeserializeObject<List<Location>>(json);

            // check
            Assert.AreEqual(locationList.Count, locationList2.Count, "length of lists must be equal");
            ////Assert.AreEqual(locationList.First(), locationList2.First(), "single entry must be equal");
        }
    }
}
