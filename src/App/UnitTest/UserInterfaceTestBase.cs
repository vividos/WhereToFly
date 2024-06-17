using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Base class for user interface based unit tests
    /// </summary>
    public class UserInterfaceTestBase
    {
        /// <summary>
        /// Sets up unit test for testing user interface
        /// </summary>
        [TestInitialize]
        public void SetUpUnitTestUserInterface()
        {
            MauiMocks.Init();

            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<IGeolocationService, UnitTestGeolocationService>();
            DependencyService.Register<SvgImageCache>();

            App.Settings = new AppSettings();
        }
    }
}
