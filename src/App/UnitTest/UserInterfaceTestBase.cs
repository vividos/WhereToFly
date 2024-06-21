using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Base class for user interface based unit tests
    /// </summary>
    public class UserInterfaceTestBase
    {
#pragma warning disable CS0612 // Type or member is obsolete
        /// <summary>
        /// Mock implementation of IFontNamedSizeService
        /// </summary>
        internal class MockFontNamedSizeService : IFontNamedSizeService
        {
            /// <inheritdoc />
            public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
            {
                return 42.0;
            }
        }
#pragma warning restore CS0612 // Type or member is obsolete

        /// <summary>
        /// Sets up unit test for testing user interface
        /// </summary>
        [TestInitialize]
        public void SetUpUnitTestUserInterface()
        {
            MauiMocks.Init();

#pragma warning disable CS0612 // Type or member is obsolete
            DependencyService.Register<IFontNamedSizeService, MockFontNamedSizeService>();
#pragma warning restore CS0612 // Type or member is obsolete

            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<IGeolocationService, UnitTestGeolocationService>();
            DependencyService.Register<SvgImageCache>();

            App.Settings = new AppSettings();
        }

        /// <summary>
        /// Cleans up unit test for testing user interface
        /// </summary>
        [TestCleanup]
        public void TearDownUnitTestUserInterface()
        {
            MauiMocks.Reset();
        }
    }
}
