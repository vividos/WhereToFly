using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using WhereToFly.App.Logic;
using WhereToFly.App.Services.SqliteDatabase;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for LayerListViewModel
    /// </summary>
    [TestClass]
    public class LayerListViewModelTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
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
            Assert.IsNotNull(viewModel.LayerList, "layer list must be available");
            Assert.IsTrue(viewModel.LayerList.Any(), "layer list must not be empty");
            Assert.IsFalse(viewModel.IsListEmpty, "layer list must not be empty");
            Assert.IsTrue(viewModel.IsClearLayerListEnabled, "layer list must not be empty");
            Assert.IsNotNull(viewModel.ImportLayerCommand, "command must not be null");
            Assert.IsNotNull(viewModel.DeleteLayerListCommand, "command must not be null");
        }
    }
}
