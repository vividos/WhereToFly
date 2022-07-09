using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.Models;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for AppSettings class
    /// </summary>
    [TestClass]
    public class AppSettingsTest
    {
        /// <summary>
        /// Tests AppSettings constructor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // run
            var appSettings = new AppSettings();

            // check
            Assert.IsFalse(appSettings.LastShownPosition.Valid, "last known position must be invalid");
        }
    }
}
