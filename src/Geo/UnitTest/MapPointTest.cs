﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.Json;
using WhereToFly.Geo.Model;
using WhereToFly.Geo.Serializers;

namespace WhereToFly.Geo.UnitTest
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

            MapPoint? noPoint = null;
            Assert.IsFalse(mapPoint1.Equals(noPoint), "map point must not be equal to null");

            object? noObject = null;
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
            var dict = new Dictionary<MapPoint, bool>
            {
                { mapPoint1, true },
            };

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

            var jsonTypeInfo = GeoModelJsonSerializerContext.Default.MapPoint;

            // run
            string json1 = JsonSerializer.Serialize(mapPoint1, jsonTypeInfo);
            string json2 = JsonSerializer.Serialize(mapPoint2, jsonTypeInfo);
            string json3 = JsonSerializer.Serialize(mapPoint3, jsonTypeInfo);

            var mapPoint1a = JsonSerializer.Deserialize(json1, jsonTypeInfo);
            var mapPoint2a = JsonSerializer.Deserialize(json2, jsonTypeInfo);
            var mapPoint3a = JsonSerializer.Deserialize(json3, jsonTypeInfo);

            // check
            Assert.IsNotNull(mapPoint1a, "returned value must not be null");
            Assert.IsNotNull(mapPoint2a, "returned value must not be null");
            Assert.IsNotNull(mapPoint3a, "returned value must not be null");

            Assert.AreEqual(mapPoint1.Latitude, mapPoint1a.Latitude, 1e-6, "latitude of map point 1 must match");
            Assert.AreEqual(mapPoint1.Longitude, mapPoint1a.Longitude, 1e-6, "longitude of map point 1 must match");
            Assert.AreEqual(mapPoint1.Altitude.HasValue, mapPoint1a.Altitude.HasValue, "altitude of map point 1 must be set");

            Assert.AreEqual(mapPoint2.Latitude, mapPoint2a.Latitude, 1e-6, "latitude of map point 2 must match");
            Assert.AreEqual(mapPoint2.Longitude, mapPoint2a.Longitude, 1e-6, "longitude of map point 2 must match");
            Assert.IsTrue(mapPoint2.Altitude.HasValue, "altitude must be set");
            Assert.IsTrue(mapPoint2a.Altitude.HasValue, "altitude must be set");
            Assert.AreEqual(mapPoint2.Altitude.HasValue, mapPoint2a.Altitude.HasValue, "altitude of map point 2 must be set");
            Assert.AreEqual(mapPoint2.Altitude.Value, mapPoint2a.Altitude.Value, 1e-6, "altitude of map point 2 must match");

            Assert.AreEqual(mapPoint3.Valid, mapPoint3a.Valid, "valid property of map point 3 must match");
        }

        /// <summary>
        /// Tests JSON deserializer handling legacy MapPoint format
        /// </summary>
        [TestMethod]
        public void TestLegacyJsonDeserializer()
        {
            // set up
            string json1 = "{\"Latitude\":47.6764385,\"Longitude\":11.8710533,\"Altitude\":1786.1,\"Valid\":true}";
            string json2 = "{\"Latitude\":0.0,\"Longitude\":0.0,\"Altitude\":0,\"Valid\":false}";

            // run
            var jsonTypeInfo = GeoModelJsonSerializerContext.Default.MapPoint;

            var mapPoint1 = JsonSerializer.Deserialize(json1, jsonTypeInfo);
            var mapPoint2 = JsonSerializer.Deserialize(json2, jsonTypeInfo);

            // check
            Assert.IsNotNull(mapPoint1, "map point 1 must not be null");
            Assert.IsTrue(mapPoint1.Valid, "map point 1 must be valid");
            Assert.IsTrue(mapPoint1.Altitude.HasValue, "map point 1 must have altitude");

            Assert.AreEqual(47.6764385, mapPoint1.Latitude, 1e-7, "deserialized latitude must match");
            Assert.AreEqual(11.8710533, mapPoint1.Longitude, 1e-7, "deserialized longitude must match");
            Assert.AreEqual(1786.1, mapPoint1.Altitude.Value, 1e-2, "deserialized altitude must match");

            Assert.IsNotNull(mapPoint2, "map point 2 must not be null");
            Assert.IsFalse(mapPoint2.Valid, "map point 2 must be invalid");
        }
    }
}
