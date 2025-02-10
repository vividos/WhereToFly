using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using WhereToFly.App.Models;
using WhereToFly.App.Serializers;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for <see cref="AppSettings"/> class
    /// </summary>
    [TestClass]
    public class AppSettingsTest
    {
        /// <summary>
        /// Tests <see cref="AppSettings"/> constructor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // run
            var appSettings = new AppSettings();

            // check
            Assert.IsFalse(appSettings.LastShownPosition.Valid, "last known position must be invalid");
        }

        /// <summary>
        /// Tests serializing and deserializing <see cref="AppSettings"/>
        /// </summary>
        [TestMethod]
        public void TestSerializing()
        {
            // set up
            var appSettings = new AppSettings();

            // run
            string json = JsonSerializer.Serialize(
                appSettings,
                ModelsJsonSerializerContext.Default.AppSettings);

            var newAppSettings = JsonSerializer.Deserialize(
                json,
                ModelsJsonSerializerContext.Default.AppSettings);

            // check
            Assert.IsNotNull(
                newAppSettings,
                "deserialized app settings must not be null");

            Assert.AreEqual(
                appSettings.LastShownPosition.Valid,
                newAppSettings.LastShownPosition.Valid,
                "valid flags must match");
        }
    }
}
