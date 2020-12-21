using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Services.SqliteDatabase;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for LayerListViewModel
    /// </summary>
    [TestClass]
    public class LayerListViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks, IPlatform and IDataService.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<IDataService, SqliteDatabaseDataService>();
            DependencyService.Register<SvgImageCache>();
        }

        /// <summary>
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up
            var viewModel = new LayerListViewModel();

            // run
            bool result = viewModel.WaitForPropertyChange(nameof(viewModel.LayerList), TimeSpan.FromSeconds(10));

            // check
            Assert.IsTrue(result, "LayerList property must have been changed");
            Assert.IsTrue(viewModel.LayerList.Any(), "layer list must not be empty");
            Assert.IsFalse(viewModel.IsListEmpty, "layer list must not be empty");
            Assert.IsTrue(viewModel.IsClearLayerListEnabled, "layer list must not be empty");
            Assert.IsNotNull(viewModel.ImportLayerCommand, "command must not be null");
            Assert.IsNotNull(viewModel.DeleteLayerListCommand, "command must not be null");
        }
    }
}
