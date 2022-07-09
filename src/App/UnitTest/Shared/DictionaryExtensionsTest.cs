using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using WhereToFly.Shared.Base;

namespace WhereToFly.App.UnitTest.Shared
{
    /// <summary>
    /// Tests for the DictionaryExtensions extension methods
    /// </summary>
    [TestClass]
    public class DictionaryExtensionsTest
    {
        /// <summary>
        /// Tests extension method GetValueOrDefault().
        /// </summary>
        [TestMethod]
        public void TestGetValueOrDefault()
        {
            // set up
            var dict = new Dictionary<string, int>
            {
                { "key1", 42 },
            };

            Dictionary<string, int> nullDict = null;

            // run + check
            int value1 = dict.GetValueOrDefault("key1", 64);
            Assert.AreEqual(42, value1, "value must match value in dictionary");

            int value2 = dict.GetValueOrDefault("key2", 64);
            Assert.AreEqual(64, value2, "value must match default value");

            Assert.ThrowsException<ArgumentNullException>(
                () => nullDict.GetValueOrDefault("key3", 128),
                "must throw eception when dict is null");
        }
    }
}
