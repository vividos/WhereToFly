using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests for TakeoffDirectionsHelper class methods
    /// </summary>
    [TestClass]
    public class TakeoffDirectionsHelperTest
    {
        /// <summary>
        /// Tests parsing correct takeoff text strings
        /// </summary>
        [TestMethod]
        public void TestTryParseCorrectTakeoffTexts()
        {
            // set up
            string[] correctTakeoffTexts = new string[]
            {
                "N",
                "SSW",
                "N-E",
                "N-SSE",
                "N-S",
                "N-SSW",
                "E-WSW",
                "E-W",
                "E-WNW",
                "N-NNW",
                "S-SW,NW-W",
                "S-SW, NW-W",
                "O-NO",
                "West takeoff (W-SW)",
                "Takeoff (N, O, S)",
                "Takeoff (NO, NW)",
            };

            foreach (var text in correctTakeoffTexts)
            {
                // run
                bool result = TakeoffDirectionsHelper.TryParse(text, out TakeoffDirections takeoffDirections);

                Debug.WriteLine($"text={text} directions={takeoffDirections}");

                // check
                Assert.IsTrue(result, "text must have been parsed correctly");
                Assert.IsTrue(takeoffDirections != TakeoffDirections.None, "takeoff direction must not be empty");
            }
        }

        /// <summary>
        /// Tests parsing invalid takeoff text strings
        /// </summary>
        [TestMethod]
        public void TestTryParseInvalidTakeoffTexts()
        {
            // set up
            string[] invalidTakeoffTexts = new string[]
            {
                "SR",
                "SSW-",
                "N-NNW-",
                "S-SW NW-W",
            };

            foreach (var text in invalidTakeoffTexts)
            {
                // run
                bool result = TakeoffDirectionsHelper.TryParse(text, out TakeoffDirections takeoffDirections);

                // check
                Assert.IsFalse(result, "text parsing must have failed");

                Assert.IsTrue(takeoffDirections == TakeoffDirections.None, "takeoff direction must be None");
            }
        }

        /// <summary>
        /// Tests method ModifyAdjacentDirectionsFromView()
        /// </summary>
        [TestMethod]
        public void TestModifyAdjacentDirectionsFromView()
        {
            // check
            Assert.AreEqual(
                TakeoffDirections.NE | TakeoffDirections.NNE | TakeoffDirections.ENE,
                TakeoffDirectionsHelper.ModifyAdjacentDirectionsFromView(TakeoffDirections.NE),
                "modified takeoff directions value must contain adjacent directions");

            Assert.AreEqual(
                TakeoffDirections.All,
                TakeoffDirectionsHelper.ModifyAdjacentDirectionsFromView(
                    TakeoffDirections.N | TakeoffDirections.NE |
                    TakeoffDirections.E | TakeoffDirections.SE |
                    TakeoffDirections.S | TakeoffDirections.SW |
                    TakeoffDirections.W | TakeoffDirections.NW),
                "modified takeoff directions value must contain all directions");
        }
    }
}
