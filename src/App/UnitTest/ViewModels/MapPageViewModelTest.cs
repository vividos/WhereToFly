using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class <see cref="MapPageViewModel"/>
    /// </summary>
    [TestClass]
    public class MapPageViewModelTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up + run + check
            try
            {
                var viewModel = new MapPageViewModel(
                    DependencyService.Get<IAppMapService>(),
                    DependencyService.Get<IGeolocationService>());
            }
            catch (Exception ex)
            {
                Assert.Fail($"ctor must not throw but did: {ex}");
            }
        }
    }
}
