using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class <see cref="MenuViewModel"/>
    /// </summary>
    [TestClass]
    public class MenuViewModelTest
    {
        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var viewModel = new MenuViewModel();

            // check
            Assert.IsTrue(viewModel.MenuItemList.Any(), "menu item list must contain items");

            foreach (var menuItem in viewModel.MenuItemList)
            {
                var icon = menuItem.Icon;
                Assert.IsNotNull(icon, "icon must not be null");
            }
        }
    }
}
