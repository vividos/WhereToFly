﻿using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Tests for MapPage class
    /// </summary>
    [TestClass]
    public class MapPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IGeolocationService, UnitTestGeolocationService>();
        }

        /// <summary>
        /// Tests default ctor of page
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var page = new MapPage();

            // check
            Assert.IsTrue(page.Title.Length > 0, "page title must have been set");
        }
    }
}
