using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using WhereToFly.Geo.Model;
using WhereToFly.WebApi.Logic;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Tests for LocationFindManager class
    /// </summary>
    [TestClass]
    public class LocationFindManagerTest
    {
        /// <summary>
        /// Tests initializing database by querying an unknown location
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestInitialize()
        {
            // set up
            var mgr = new LocationFindManager();

            // run
            Location location = await mgr.GetAsync("xyz123");

            // check
            Assert.IsNull(location, "location must not exist");
        }
    }
}
