using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.Json;
using WhereToFly.Shared.Model;
using WhereToFly.Shared.Model.Serializers;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for <see cref="AppConfig"/> class
    /// </summary>
    [TestClass]
    public class AppConfigTest
    {
        /// <summary>
        /// Tests serializing and deserializing <see cref="AppConfig"/>
        /// </summary>
        [TestMethod]
        public void TestSerializingDeserializing()
        {
            // set up
            var appConfig = new AppConfig
            {
                ApiKeys = new Dictionary<string, string>
                {
                    { "testApiKey1", "123456" },
                    { "testApiKey2", "00000000" },
                },
                ExpiryDate = DateTimeOffset.Now,
            };

            // run
            string json = JsonSerializer.Serialize(
                appConfig,
                SharedModelJsonSerializerContext.Default.AppConfig);

            var newAppConfig = JsonSerializer.Deserialize(
                json,
                SharedModelJsonSerializerContext.Default.AppConfig);

            // check
            Assert.IsNotNull(
                newAppConfig,
                "deserialized app config must not be null");

            Assert.AreEqual(
                appConfig.ExpiryDate,
                newAppConfig.ExpiryDate,
                "epiry date must match");

            Assert.AreEqual(
                string.Join(",", newAppConfig.ApiKeys.Keys),
                string.Join(",", appConfig.ApiKeys.Keys),
                "api keys must match");

            Assert.AreEqual(
                string.Join(",", newAppConfig.ApiKeys.Values),
                string.Join(",", appConfig.ApiKeys.Values),
                "api key values must match");
        }

        /// <summary>
        /// Tests deserializing <see cref="AppConfig"/> from JSON
        /// </summary>
        [TestMethod]
        public void TestDeserializingFromJson()
        {
            // set up
            string json = """
                {
                    "apiKeys": {
                        "CesiumIonApiKey": "xyz",
                        "BingMapsApiKey": "123"
                    },
                    "expiryDate": "2025-02-18T07:29:02.8118763+00:00"
                }
                """;

            // run
            var appConfig = JsonSerializer.Deserialize(
                json,
                SharedModelJsonSerializerContext.Default.AppConfig);

            // check
            Assert.IsNotNull(
                appConfig,
                "deserialized app config must not be null");

            Assert.IsTrue(
                appConfig.ExpiryDate != DateTimeOffset.MinValue,
                "epiry date must be valid");

            Assert.AreEqual(
                "xyz",
                appConfig.ApiKeys["CesiumIonApiKey"],
                "api key 1 must match");

            Assert.AreEqual(
                "123",
                appConfig.ApiKeys["BingMapsApiKey"],
                "api key 2 must match");
        }
    }
}
