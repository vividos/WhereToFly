using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class ImportLocationsViewModel
    /// </summary>
    [TestClass]
    public class ImportLocationsViewModelTest
    {
        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var viewModel = new ImportLocationsViewModel();

            // check
            Assert.IsNotNull(viewModel.ImportIncludedCommand, "import included command must be non-null");
            Assert.IsNotNull(viewModel.ImportFromStorageCommand, "import from storage command must be non-null");
            Assert.IsNotNull(viewModel.DownloadFromWebCommand, "download from web command must be non-null");
        }
    }
}
