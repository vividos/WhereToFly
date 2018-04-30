using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using WhereToFly.App.Model;

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
        /// Tests property Valid, with invalid coordinates
        /// </summary>
        [TestMethod]
        public void TestInvalid()
        {
            // run
            var mapPoint = new MapPoint(0.0, 0.0);

            // check
            Assert.IsFalse(mapPoint.Valid, "map point must be invalid");
        }

        /// <summary>
        /// Tests method ToString()
        /// </summary>
        [TestMethod]
        public void TestToString()
        {
            // set up
            double latitude = 47.6764385;
            double longitude = 11.8710533;

            // run
            var mapPoint = new MapPoint(latitude, longitude);
            string text = mapPoint.ToString();

            // check
            Assert.IsTrue(text.Length > 0, "map point text must contain characters");
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
    }
}
