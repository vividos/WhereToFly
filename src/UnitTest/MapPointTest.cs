using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Logic.Model;

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
    }
}
