using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels;

/// <summary>
/// Unit tests for LayerListViewModel
/// </summary>
[TestClass]
public class LayerListViewModelTest : UserInterfaceTestBase
{
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
        Assert.IsNotEmpty(viewModel.LayerList, "layer list must not be empty");
        Assert.IsFalse(viewModel.IsListEmpty, "layer list must not be empty");
        Assert.IsTrue(viewModel.IsClearLayerListEnabled, "layer list must not be empty");
        Assert.IsNotNull(viewModel.ImportLayerCommand, "command must not be null");
        Assert.IsNotNull(viewModel.DeleteLayerListCommand, "command must not be null");
    }
}
