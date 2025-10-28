using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.Abstractions;
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
                    new UnitTestMapView(),
                    this.Services.GetRequiredService<IAppMapService>(),
                    this.Services.GetRequiredService<IDataService>(),
                    this.Services.GetRequiredService<IGeolocationService>());

                Assert.IsNotNull(viewModel, "view model most have been creted");
            }
            catch (Exception ex)
            {
                Assert.Fail($"ctor must not throw but did: {ex}");
            }
        }
    }
}
