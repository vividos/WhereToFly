using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for class MapPoint
    /// </summary>
    [TestClass]
    public class MapPointTest
    {
        /// <summary>
        /// Tests MapPoint constructor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up
            double latitude = 47.6764385;
            double longitude = 11.8710533;

            // run
            var mapPoint = new MapPoint(latitude, longitude);

            // check
            Assert.IsTrue(mapPoint.Valid, "map point must be valid");
            Assert.AreEqual(latitude, mapPoint.Latitude, double.Epsilon, "latitude must match");
            Assert.AreEqual(longitude, mapPoint.Longitude, double.Epsilon, "longitude must match");
        }

        /// <summary>
        /// Tests ctor, with invalid coordinates
        /// </summary>
        [TestMethod]
        public void TestInvalid()
        {
            // set up
            var point = new MapPoint(0.0, 0.0);

            // check
            Assert.IsFalse(point.Valid, "coordinate must be invalid");
            Assert.AreEqual(0.0, point.Latitude, double.Epsilon, "latitude must be 0.0");
            Assert.AreEqual(0.0, point.Longitude, double.Epsilon, "longitude must be 0.0");
            Assert.IsFalse(point.Altitude.HasValue, "altitude must not be set");
            Assert.AreEqual("invalid", point.ToString(), "invalid point must return correct ToString() result");
        }

        /// <summary>
        /// Tests ToString() method
        /// </summary>
        [TestMethod]
        public void TestToString()
        {
            // set up
            var point1 = new MapPoint(48.21231, 11.56078);
            var point2 = new MapPoint(point1.Latitude, point1.Longitude, 1257.2);

            // run
            string text1 = point1.ToString();
            string text2 = point2.ToString();

            // check
            Assert.AreEqual("Lat=48.212310, Long=11.560780, Alt=N/A", text1, "ToString() text must be correct");
            Assert.AreEqual("Lat=48.212310, Long=11.560780, Alt=1257.20", text2, "ToString() text must be correct");
        }

        /// <summary>
        /// Tests MapPoint equality
        /// </summary>
        [TestMethod]
        public void TestEquality()
        {
            // set up
            double latitude = 47.6764385;
            double longitude = 11.8710533;

            // run
            var mapPoint1 = new MapPoint(latitude, longitude);
            var mapPoint2 = new MapPoint(latitude, longitude);
            var mapPoint3 = new MapPoint(latitude, -longitude);
            var mapPoint4 = new MapPoint(0.0, 0.0);

            // check
            Assert.IsTrue(mapPoint1.Equals(mapPoint2), "map points 1 and 2 must be equal");
            Assert.IsTrue(mapPoint1.Equals((object)mapPoint2), "map points 1 and 2 must be equal");
            Assert.IsFalse(mapPoint1.Equals(mapPoint3), "map points 1 and 3 must not be equal");
            Assert.IsFalse(mapPoint1.Equals(mapPoint4), "map point must not be equal to invalid map point");

            MapPoint noPoint = null;
            Assert.IsFalse(mapPoint1.Equals(noPoint), "map point must not be equal to null");

            object noObject = null;
            Assert.IsFalse(mapPoint1.Equals(noObject), "map point must not be equal to null");
            Assert.IsFalse(mapPoint1.Equals("text"), "map point must not be equal to different type");
        }

        /// <summary>
        /// Tests MapPoint GetHashCode() function
        /// </summary>
        [TestMethod]
        public void TestGetHashCode()
        {
            // set up
            double latitude = 47.6764385;
            double longitude = 11.8710533;

            var mapPoint1 = new MapPoint(latitude, longitude);
            var mapPoint2 = new MapPoint(latitude, longitude);
            var mapPoint3 = new MapPoint(latitude, -longitude);
            var mapPoint4 = new MapPoint(0.0, 0.0);

            // run + check
            var dict = new Dictionary<MapPoint, bool>();
            Assert.IsFalse(dict.ContainsKey(mapPoint1), "map point must not be in dictionary yet");

            dict.Add(mapPoint1, true);

            Assert.IsTrue(dict.ContainsKey(mapPoint1), "map point must be in dictionary now");
            Assert.IsTrue(dict.ContainsKey(mapPoint2), "map point in different reference must also be found in dict");
            Assert.IsFalse(dict.ContainsKey(mapPoint3), "different map point must not be found in dict");
            Assert.IsFalse(dict.ContainsKey(mapPoint4), "invalid map point must not be found in dict");

            dict.Remove(mapPoint2);
            Assert.IsFalse(dict.ContainsKey(mapPoint1), "map point must not be in dictionary now");
            Assert.IsFalse(dict.ContainsKey(mapPoint2), "map point in different reference must also not be found in dict");
        }

        /// <summary>
        /// Tests custom JSON serializer
        /// </summary>
        [TestMethod]
        public void TestJsonSerializer()
        {
            // set up
            double latitude = 47.6764385;
            double longitude = 11.8710533;
            double altitude = 1786.1;

            var mapPoint1 = new MapPoint(latitude, longitude);
            var mapPoint2 = new MapPoint(latitude, longitude, altitude);
            var mapPoint3 = new MapPoint(0.0, 0.0);

            // run
            string json1 = JsonConvert.SerializeObject(mapPoint1);
            string json2 = JsonConvert.SerializeObject(mapPoint2);
            string json3 = JsonConvert.SerializeObject(mapPoint3);

            var mapPoint1a = JsonConvert.DeserializeObject<MapPoint>(json1);
            var mapPoint2a = JsonConvert.DeserializeObject<MapPoint>(json2);
            var mapPoint3a = JsonConvert.DeserializeObject<MapPoint>(json3);

            // check
            Assert.AreEqual(mapPoint1.Latitude, mapPoint1a.Latitude, 1e-6, "latitude of map point 1 must match");
            Assert.AreEqual(mapPoint1.Longitude, mapPoint1a.Longitude, 1e-6, "longitude of map point 1 must match");
            Assert.AreEqual(mapPoint1.Altitude.HasValue, mapPoint1a.Altitude.HasValue, "altitude of map point 1 must be set");

            Assert.AreEqual(mapPoint2.Latitude, mapPoint2a.Latitude, 1e-6, "latitude of map point 2 must match");
            Assert.AreEqual(mapPoint2.Longitude, mapPoint2a.Longitude, 1e-6, "longitude of map point 2 must match");
            Assert.AreEqual(mapPoint2.Altitude.HasValue, mapPoint2a.Altitude.HasValue, "altitude of map point 2 must be set");
            Assert.AreEqual(mapPoint2.Altitude.Value, mapPoint2a.Altitude.Value, 1e-6, "altitude of map point 2 must match");

            Assert.AreEqual(mapPoint3.Valid, mapPoint3a.Valid, "valid property of map point 3 must match");
        }
    }
}
