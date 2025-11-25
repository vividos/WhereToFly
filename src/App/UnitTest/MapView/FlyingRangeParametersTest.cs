using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.MapView.Models;

namespace WhereToFly.App.UnitTest.MapView
{
    /// <summary>
    /// Tests for <see cref="FlyingRangeParameters"/> class
    /// </summary>
    [TestClass]
    public class FlyingRangeParametersTest
    {
        /// <summary>
        /// Test parameters 1; equal to test params 2
        /// </summary>
        private static readonly FlyingRangeParameters TestParams1 = new()
        {
            GlideRatio = 7.0,
            GliderSpeed = 37,
        };

        /// <summary>
        /// Test parameters 2; equal to test params 1
        /// </summary>
        private static readonly FlyingRangeParameters TestParams2 = new()
        {
            GlideRatio = 7.0,
            GliderSpeed = 37,
        };

        /// <summary>
        /// Test parameters 3; unequal to test params 1 and 2
        /// </summary>
        private static readonly FlyingRangeParameters TestParams3 = new()
        {
            GlideRatio = 6.5,
            GliderSpeed = 38,
        };

        /// <summary>
        /// Tests equality of flying range parameters
        /// </summary>
        [TestMethod]
        public void TestEquality()
        {
            // check
            Assert.AreEqual(TestParams1, TestParams2, "parameters 1 and 2 must be equal");
            Assert.AreNotEqual(TestParams1, TestParams3, "parameters 1 and 3 must be unequal");
            Assert.AreNotEqual(TestParams2, TestParams3, "parameters 2 and 3 must be unequal");
        }

        /// <summary>
        /// Tests equality operators of flying range parameters
        /// </summary>
        [TestMethod]
        public void TestEqualityOperators()
        {
            // check
            Assert.IsTrue(TestParams1 == TestParams2, "parameters 1 and 2 must be equal");
            Assert.IsFalse(TestParams1 != TestParams2, "parameters 1 and 2 must not be unequal");
            Assert.IsFalse(TestParams1 == TestParams3, "parameters 1 and 3 must not be equal");
            Assert.IsTrue(TestParams1 != TestParams3, "parameters 1 and 3 must be unequal");
            Assert.IsFalse(TestParams2 == TestParams3, "parameters 2 and 3 must not be equal");
            Assert.IsTrue(TestParams2 != TestParams3, "parameters 2 and 3 must be unequal");
        }

        /// <summary>
        /// Tests GetHashCode() method
        /// </summary>
        [TestMethod]
        public void TestGetHashCode()
        {
            // check
            Assert.AreEqual(
                TestParams1.GetHashCode(),
                TestParams2.GetHashCode(),
                "parameters 1 and 2 hash codes must be equal");

            Assert.AreNotEqual(
                TestParams1.GetHashCode(),
                TestParams3.GetHashCode(),
                "parameters 1 and 3 hash codes must be unequal");

            Assert.AreNotEqual(
                TestParams2.GetHashCode(),
                TestParams3.GetHashCode(),
                "parameters 2 and 3 hash codes must be unequal");
        }

        /// <summary>
        /// Tests ToString() method
        /// </summary>
        [TestMethod]
        public void TestToString()
        {
            // check
            Assert.IsGreaterThan(0, TestParams1.ToString().Length, "ToString() must return non-empty string");
            Assert.IsGreaterThan(0, TestParams2.ToString().Length, "ToString() must return non-empty string");
            Assert.IsGreaterThan(0, TestParams3.ToString().Length, "ToString() must return non-empty string");
        }
    }
}
