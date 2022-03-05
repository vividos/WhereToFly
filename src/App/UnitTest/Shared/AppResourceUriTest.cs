using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.UnitTest.Shared
{
    /// <summary>
    /// Tests for class AppResourceUri
    /// </summary>
    [TestClass]
    public class AppResourceUriTest
    {
        /// <summary>
        /// Tests constructing an AppResourceUri from an URI string
        /// </summary>
        [TestMethod]
        public void TestCreateUriFromString()
        {
            // set up
            string uriText1 = "where-to-fly://FindMeSpotPos/xxx";
            string uriText2 = "where-to-fly://GarminInreachPos/yyy";

            // run
            var uri1 = new AppResourceUri(uriText1);
            var uri2 = new AppResourceUri(uriText2);

            // check
            Assert.IsTrue(uri1.IsValid, "uri1 must be valid");
            Assert.IsTrue(uri2.IsValid, "uri2 must be valid");

            Assert.IsTrue(uriText1.ToLowerInvariant() == uri1.ToString(), "uri text and ToString() result must match");
            Assert.IsTrue(uriText2.ToLowerInvariant() == uri2.ToString(), "uri text and ToString() result must match");
        }

        /// <summary>
        /// Tests constructing an AppResourceUri from resource type and data
        /// </summary>
        [TestMethod]
        public void TestCreateUriFromTypeAndData()
        {
            // run
            var uri1 = new AppResourceUri(AppResourceUri.ResourceType.FindMeSpotPos, "xxx");
            var uri2 = new AppResourceUri(AppResourceUri.ResourceType.GarminInreachPos, "yyy");

            // check
            Assert.IsTrue(uri1.IsValid, "uri1 must be valid");
            Assert.IsTrue(uri2.IsValid, "uri2 must be valid");

            Assert.AreEqual("where-to-fly://findmespotpos/xxx", uri1.ToString(), "uri text and ToString() result must match");
            Assert.AreEqual("where-to-fly://garmininreachpos/yyy", uri2.ToString(), "uri text and ToString() result must match");
        }

        /// <summary>
        /// Tests parsing invalid Uris
        /// </summary>
        [TestMethod]
        public void TestParseInvalidUris()
        {
            // run
            Assert.ThrowsException<ArgumentNullException>(() => new AppResourceUri(null));
            Assert.ThrowsException<ArgumentException>(() => new AppResourceUri(AppResourceUri.ResourceType.None, "123"));
            Assert.ThrowsException<ArgumentNullException>(() => new AppResourceUri(AppResourceUri.ResourceType.FindMeSpotPos, null));
            Assert.ThrowsException<UriFormatException>(() => new AppResourceUri(string.Empty));

            var uri1 = new AppResourceUri("https://github.com/");
            var uri2 = new AppResourceUri("where-to-fly://");
            var uri3 = new AppResourceUri("where-to-fly://xxx");
            var uri4 = new AppResourceUri("where-to-fly://findmespotpos/");

            // check
            Assert.IsFalse(uri1.IsValid, "uri must be invalid");
            Assert.IsFalse(uri2.IsValid, "uri must be invalid");
            Assert.IsFalse(uri3.IsValid, "uri must be invalid");
            Assert.IsFalse(uri4.IsValid, "uri must be invalid");

            Assert.AreEqual("invalid", uri1.ToString(), "invalid URI must return proper ToString() result");
        }

        /// <summary>
        /// Tests equality of AppResourceUri objects
        /// </summary>
        [TestMethod]
        public void TestEquality()
        {
            // set up
            var uri1 = new AppResourceUri(AppResourceUri.ResourceType.FindMeSpotPos, "xxx");
            var uri2 = new AppResourceUri(AppResourceUri.ResourceType.GarminInreachPos, "yyy");
            var uri3 = new AppResourceUri(AppResourceUri.ResourceType.GarminInreachPos, "yyy");

            // run
            Assert.AreEqual<object>(uri1, uri1, "same objects must be equal");
            Assert.AreNotEqual<object>(uri1, uri2, "different objects must be equal");
            Assert.AreEqual<object>(uri2, uri3, "different references must be equal");
            Assert.AreNotEqual<object>("hello", uri1, "different object types must not be equal");

            Assert.AreEqual<AppResourceUri>(uri1, uri1, "same objects must be equal");
            Assert.AreNotEqual<AppResourceUri>(uri1, uri2, "different objects must be equal");
            Assert.AreEqual<AppResourceUri>(uri2, uri3, "different references must be equal");
            Assert.AreNotEqual<AppResourceUri>(null, uri1, "object must not be equal to null");

            Assert.AreEqual(uri2.GetHashCode(), uri3.GetHashCode(), "hash codes of same objects must be equal");
        }

        /// <summary>
        /// Tests serializing and deserializing AppResourceUri objects
        /// </summary>
        [TestMethod]
        public void TestSerializeDeserialize()
        {
            // set up
            var uri = new AppResourceUri("where-to-fly://GarminInreachPos/yyy");

            // run
            string json = JsonConvert.SerializeObject(uri);
            AppResourceUri uri2 = JsonConvert.DeserializeObject<AppResourceUri>(json);

            // check
            Assert.IsTrue(uri2.IsValid, "deserialized uri must be valid");
            Assert.AreEqual(uri, uri2, "uris must be equal");
        }

        /// <summary>
        /// Tests app resource URI as key in dictionies
        /// </summary>
        public void TestKeyInDictionary()
        {
            // set up
            var uri1 = new AppResourceUri(AppResourceUri.ResourceType.FindMeSpotPos, "xxx");
            var uri2 = new AppResourceUri(AppResourceUri.ResourceType.GarminInreachPos, "yyy");
            var uri3 = new AppResourceUri(AppResourceUri.ResourceType.GarminInreachPos, "yyy");

            var dict = new Dictionary<AppResourceUri, int>();

            // run + check
            Assert.IsFalse(dict.Any(), "there must be no items in the dictionary");

            dict[uri1] = 42;
            Assert.IsTrue(dict.Any(), "there must be an item in the dictionary");
            Assert.IsTrue(dict.ContainsKey(uri1), "dict must contain value for uri1");
            Assert.IsFalse(dict.ContainsKey(uri2), "dict must not contain value for uri1");

            dict[uri2] = 64;
            Assert.AreEqual(2, dict.Count, "there must be 2 items in the dictionary");

            dict[uri3] = 128;
            Assert.AreEqual(2, dict.Count, "there must still be 2 items in the dictionary");

            Assert.AreEqual(42, dict[uri1], "value of key with uri 1 must match");
            Assert.AreEqual(128, dict[uri2], "value of key with uri 2 must match");
            Assert.AreEqual(128, dict[uri3], "value of key with uri 3 must match");
        }
    }
}
