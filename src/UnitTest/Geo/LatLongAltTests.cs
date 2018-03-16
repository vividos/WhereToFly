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
            var westPoint = centerPoint.PolarOffset(distanceInMeter, 270, 0.0);
            var southPoint = centerPoint.PolarOffset(distanceInMeter, 180, 0.0);
            var anglePoint = centerPoint.PolarOffset(distanceInMeter, 71, 0.0);

            // check
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(northPoint), "distance must match");
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(eastPoint), "distance must match");
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(westPoint), "distance must match");
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(southPoint), "distance must match");
            Assert.AreEqual(distanceInMeter, centerPoint.DistanceTo(anglePoint), "distance must match");
        }
    }
}
