using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests for class MapPoint extension methods
    /// </summary>
    [TestClass]
    public class MapPointExtensionMethodsTests
    {
        /// <summary>
        /// Tests DistanceTo() method
        /// </summary>
        [TestMethod]
        public void TestDistanceTo()
        {
            // set up
            var point1 = new MapPoint(48.2, 11.5);
            var point2 = new MapPoint(48.2 + 0.1, 11.5);

            var distanceInMeter = 1000;
            var point3 = point2.PolarOffset(distanceInMeter, 321.0, 0.0);

            // run
            double distance12 = point1.DistanceTo(point2);
            double distance23 = point2.DistanceTo(point3);

            // check
            Assert.AreEqual(11132.0, distance12, 1.0, "distance from point 1 to point 2 must be correct");
            Assert.AreEqual(distanceInMeter, distance23, 0.1, "distance from point 2 to point 3 must be correct");
        }

        /// <summary>
        /// Tests CourseTo() method
        /// </summary>
        [TestMethod]
        public void TestCourseTo()
        {
            // set up
            var centerPoint = new MapPoint(48.2, 11.5);
            var northPoint = new MapPoint(48.2 + 0.1, 11.5);
            var eastPoint = new MapPoint(48.2, 11.5 + 0.1);
            var southPoint = new MapPoint(48.2 - 0.1, 11.5);
            var westPoint = new MapPoint(48.2, 11.5 - 0.1);
            var anglePoint = new MapPoint(48.2 - 0.05, 11.5 + 0.11);
            var northPole = new MapPoint(90.0, 0.0);
            var southPole = new MapPoint(-90.0, 0.0);

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
            var centerPoint = new MapPoint(48.2, 11.5);
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

        /// <summary>
        /// Tests method Offset()
        /// </summary>
        [TestMethod]
        public void TestOffset()
        {
            // set up
            var point1 = new MapPoint(48.2, 11.5);
            var distanceInMeterNorth = 3000;
            var distanceInMeterEast = 4000;

            // run
            var point2 = point1.Offset(distanceInMeterNorth, distanceInMeterEast, 0.0);
            double distance = point1.DistanceTo(point2);

            // check
            Assert.AreEqual(5000.0, distance, 0.1, "calculated distance must be correct");
        }
    }
}
