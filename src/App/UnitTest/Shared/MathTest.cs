using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.Shared.Base;

namespace WhereToFly.App.UnitTest.Shared
{
    /// <summary>
    /// Tests for the Math methods in WhereToFly.Shared.Base
    /// </summary>
    [TestClass]
    public class MathTest
    {
        /// <summary>
        /// Tests method Interpolate().
        /// </summary>
        [TestMethod]
        public void TestInterpolate()
        {
            // check
            var value1 = Math.Interpolate(0.0, 10.0, 0.42);
            var value2 = Math.Interpolate(0.0, 1.0, -2);
            var value3 = Math.Interpolate(10.0, 0.0, 0.99);

            // check
            Assert.AreEqual(4.2, value1, 1e-5, "interpolated value must match");
            Assert.AreEqual(-2.0, value2, 1e-5, "interpolated value must match");
            Assert.AreEqual(0.1, value3, 1e-5, "interpolated value must match");
        }
    }
}
