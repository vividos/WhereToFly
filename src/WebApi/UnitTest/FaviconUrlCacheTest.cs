using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using WhereToFly.WebApi.Logic;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Tests for the Favicon URL cache class
    /// </summary>
    [TestClass]
    public class FaviconUrlCacheTest
    {
        /// <summary>
        /// Tests getting a favicon URL
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestFaviconUrl()
        {
            // set up
            var cache = new FaviconUrlCache();

            // run
            var result = await cache.GetFaviconUrlAsync(new Uri("https://github.com/vividos/WhereToFly"));

            // check
            Assert.IsTrue(result.Length > 0, "result must contain text");
        }

        /// <summary>
        /// Tests getting a pre-cached favicon URL
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestPreCachedFaviconUrl()
        {
            // set up
            var cache = new FaviconUrlCache();

            // run
            var result = await cache.GetFaviconUrlAsync(new Uri("https://www.wetteronline.de/"));

            // check
            Assert.IsTrue(result.Length > 0, "result must contain text");
        }
    }
}
