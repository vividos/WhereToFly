using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.WebApi.Logic;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Unit tests for LiveWaypointID class
    /// </summary>
    [TestClass]
    public class LiveWaypointIDTest
    {
        /// <summary>
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // run
            var id = new LiveWaypointID(LiveWaypointID.WaypointType.FindMeSpot);

            // check
            Assert.AreEqual(LiveWaypointID.WaypointType.FindMeSpot, id.Type, "type must be correct");
            Assert.AreEqual(string.Empty, id.Data, "data must be empty");
        }

        /// <summary>
        /// Tests ToString() method
        /// </summary>
        [TestMethod]
        public void TestToString()
        {
            // run
            var id = new LiveWaypointID(LiveWaypointID.WaypointType.FindMeSpot);
            id.Data = "abc123";

            // check
            Assert.AreEqual("wheretofly-findmespot-abc123", id.ToString(), "ToString() text must match");
        }

        /// <summary>
        /// Tests FromString() method
        /// </summary>
        [TestMethod]
        public void TestFromString()
        {
            // run
            string textID = "wheretofly-findmespot-abc123";
            var id = LiveWaypointID.FromString(textID);

            // check
            Assert.AreEqual(LiveWaypointID.WaypointType.FindMeSpot, id.Type, "type must be correct");
            Assert.AreEqual("abc123", id.Data, "data must match");
            Assert.AreEqual(textID, id.ToString(), "ID roundtrip must be correct");
        }
    }
}
