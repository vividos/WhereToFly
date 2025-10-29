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

#pragma warning disable S1075 // URIs should not be hardcoded
            const string url = "https://github.com/vividos/WhereToFly";
#pragma warning restore S1075 // URIs should not be hardcoded

            // run
            string result = await cache.GetFaviconUrlAsync(new Uri(url));

            // check
            Assert.IsGreaterThan(0, result.Length, "result must contain text");
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

#pragma warning disable S1075 // URIs should not be hardcoded
            const string url = "https://www.wetteronline.de/";
#pragma warning restore S1075 // URIs should not be hardcoded

            // run
            string result = await cache.GetFaviconUrlAsync(new Uri(url));

            // check
            Assert.IsGreaterThan(0, result.Length, "result must contain text");
        }
    }
}
