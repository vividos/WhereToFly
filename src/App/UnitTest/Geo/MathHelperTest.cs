using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.Geo;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests for the Math helper methods in WhereToFly.Geo
    /// </summary>
    [TestClass]
    public class MathHelperTest
    {
        /// <summary>
        /// Tests method Interpolate().
        /// </summary>
        [TestMethod]
        public void TestInterpolate()
        {
            // run
            double value1 = MathHelper.Interpolate(0.0, 10.0, 0.42);
            double value2 = MathHelper.Interpolate(0.0, 1.0, -2);
            double value3 = MathHelper.Interpolate(10.0, 0.0, 0.99);

            // check
            Assert.AreEqual(4.2, value1, 1e-5, "interpolated value must match");
            Assert.AreEqual(-2.0, value2, 1e-5, "interpolated value must match");
            Assert.AreEqual(0.1, value3, 1e-5, "interpolated value must match");
        }
    }
}
