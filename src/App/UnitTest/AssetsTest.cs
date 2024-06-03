using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using WhereToFly.App.Resources;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests class Assets from the WhereToFly.App.Resources project
    /// </summary>
    [TestClass]

    public class AssetsTest
    {
        /// <summary>
        /// Tests getting stream of assets file
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        public async Task TestGetStream()
        {
            // run
            using var stream = await Assets.Get("info/Changelog.md");
            Assert.IsNotNull(stream, "stream must be available");

            using var reader = new StreamReader(stream);
            string text = await reader.ReadToEndAsync();

            // check
            Assert.IsTrue(text.Length > 0, "changelog file must have been read");
        }
    }
}
