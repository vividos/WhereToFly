using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class AddLayerPopupViewModel
    /// </summary>
    [TestClass]
    public class AddLayerPopupViewModelTest
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
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var layer = UnitTestHelper.GetDefaultLayer();

            // run
            var viewModel = new AddLayerPopupViewModel(layer);

            // check
            Assert.AreEqual(layer.Name, viewModel.LayerName, "layer name must match");

            // modify values
            viewModel.LayerName = "Layer2";
        }
    }
}
