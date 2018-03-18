using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.Geo.Spatial;

namespace WhereToFly.Geo.UnitTest
{
    /// <summary>
    /// Tests for class LatLongAlt
    /// </summary>
    [TestClass]
    public class LatLongAltTests
    {
        /// <summary>
        /// Tests ctor, with invalid coordinates
        /// </summary>
        public void TestCtor()
        {
            // set up
            var point = new LatLongAlt(0.0, 0.0);

            // check
            Assert.IsFalse(point.Valid, "coordinate must be invalid");
            Assert.AreEqual(0.0, point.Latitude, double.Epsilon, "latitude must be 0.0");
            Assert.AreEqual(0.0, point.Longitude, double.Epsilon, "longitude must be 0.0");
            Assert.IsFalse(point.Altitude.HasValue, "altitude must not be set");
        }

        /// <summary>
        /// Tests CourseTo() method
        /// </summary>
        [TestMethod]
        public void TestCourseTo()
        {
            // set up
            var centerPoint = new LatLongAlt(48.2, 11.5);
            var northPoint = new LatLongAlt(48.2 + 0.1, 11.5);
            var eastPoint = new LatLongAlt(48.2, 11.5 + 0.1);
            var southPoint = new LatLongAlt(48.2 - 0.1, 11.5);
            var westPoint = new LatLongAlt(48.2, 11.5 - 0.1);
            var anglePoint = new LatLongAlt(48.2 - 0.05, 11.5 + 0.11);
            var northPole = new LatLongAlt(90.0, 0.0);
            var southPole = new LatLongAlt(-90.0, 0.0);

            // run
            double angleToSelf = centerPoint.CourseTo(centerPoint);
            double angleToNorth = centerPoint.CourseTo(northPoint);
            double angleToEast = centerPoint.CourseTo(eastPoint);
            double angleToSouth = centerPoint.CourseTo(southPoint);
            double angleToWest = centerPoint.CourseTo(westPoint);
            double angleToOther = centerPoint.CourseTo(anglePoint);

            double angleFromNorthPole = northPole.CourseTo(centerPoint);
            double angleFromSouthPole = southPole.CourseTo(centerPoint);

            // check
            Assert.AreEqual(0.0, angleToSelf, 0.1, "angle to self must be 0.0 degrees");
            Assert.AreEqual(0.0, angleToNorth, 0.1, "angle to north must be 0.0 degrees");
            Assert.AreEqual(90.0, angleToEast, 0.1, "angle to east must be 90.0 degrees");
            Assert.AreEqual(180.0, angleToSouth, 0.1, "angle to self must be 180.0 degrees");
            Assert.AreEqual(270.0, angleToWest, 0.1, "angle to west must be 270.0 degrees");
            Assert.AreEqual(124.2, angleToOther, 0.1, "angle to other must be 124.2 degrees");
            Assert.AreEqual(180.0, angleFromNorthPole, 0.1, "angle from north pole must be 180.0 degrees");
            Assert.AreEqual(0.0, angleFromSouthPole, 0.1, "angle from south pole must be 0.0 degrees");
        }

        /// <summary>
        /// Tests method PolarOffset()
        /// </summary>
        [TestMethod]
        public void TestPolarOffset()
        {
            // set up
            var centerPoint = new LatLongAlt(48.2, 11.5);
            var distanceInMeter = 1000;

            // run
            var northPoint = centerPoint.PolarOffset(distanceInMeter, 0, 0.0);
            var eastPoint = centerPoint.PolarOffset(distanceInMeter, 90, 0.0);
            var southPoint = centerPoint.PolarOffset(distanceInMeter, 180, 0.0);
            var westPoint = centerPoint.PolarOffset(distanceInMeter, 270, 0.0);
            var anglePoint = centerPoint.PolarOffset(distanceInMeter, 71, 0.0);

            // check
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(northPoint), 1e-6, "distance must match");
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(eastPoint), 1e-6, "distance must match");
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(southPoint), 1e-6, "distance must match");
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(westPoint), 1e-6, "distance must match");
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(anglePoint), 1e-6, "distance must match");
        }
    }
}
