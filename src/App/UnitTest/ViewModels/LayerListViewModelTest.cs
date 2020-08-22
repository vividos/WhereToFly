using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
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
        }

        /// <summary>
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up
            var viewModel = new LayerListViewModel();

            var propertyChangedEvent = new ManualResetEvent(false);

            viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(viewModel.LayerList))
                    {
                        propertyChangedEvent.Set();
                    }
                };

            propertyChangedEvent.WaitOne();

            // check
            Assert.IsTrue(viewModel.LayerList.Any(), "layer list must not be empty");
            Assert.IsFalse(viewModel.IsListEmpty, "layer list must not be empty");
            Assert.IsTrue(viewModel.IsClearLayerListEnabled, "layer list must not be empty");
            Assert.IsNotNull(viewModel.ImportLayerCommand, "command must not be null");
            Assert.IsNotNull(viewModel.DeleteLayerListCommand, "command must not be null");
        }
    }
}
