using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for LayerListViewModel
    /// </summary>
    [TestClass]
    public class LayerListViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
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
            Assert.IsFalse(viewModel.LayerList.Any(), "layer list must be empty");
            Assert.IsTrue(viewModel.IsListEmpty, "layer list must be empty");
            Assert.IsFalse(viewModel.IsClearLayerListEnabled, "layer list must be empty");
            Assert.IsNotNull(viewModel.ImportLayerCommand, "command must not be null");
            Assert.IsNotNull(viewModel.DeleteLayerListCommand, "command must not be null");
        }
    }
}
