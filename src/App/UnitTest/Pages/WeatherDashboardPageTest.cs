﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Pages;

namespace WhereToFly.App.UnitTest.Pages
{
    /// <summary>
    /// Unit tests for <see cref="WeatherDashboardPage"/> class
    /// </summary>
    [TestClass]
    public class WeatherDashboardPageTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of page
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var page = new WeatherDashboardPage();

            // check
            Assert.IsTrue(
                page.Title.Length > 0,
                "page title must have been set");
        }
    }
}
