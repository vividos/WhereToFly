using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WhereToFly.App.Core.Views;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Unit tests for MapView
    /// </summary>
    [TestClass]
    public class MapViewTest
    {
        /// <summary>
        /// Tests deserializing MapView.AddFindResultParameter from JSON
        /// </summary>
        [TestMethod]
        public void TestDeserialize_AddFindResultParameter()
        {
            // set up
            string jsonParameters = "{ name: 'find result', latitude: 48.2, longitude: 11.8 }";

            // run
            var parameters = JsonConvert.DeserializeObject<MapView.AddFindResultParameter>(jsonParameters);

            // check
            Assert.AreEqual("find result", parameters.Name, "deserialized name must match");
            Assert.AreEqual(48.2, parameters.Latitude, 1e-6, "deserialized latitude must match");
            Assert.AreEqual(11.8, parameters.Longitude, 1e-6, "deserialized longitude must match");
        }
    }
}
