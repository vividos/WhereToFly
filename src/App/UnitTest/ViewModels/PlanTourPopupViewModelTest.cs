﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.ViewModels;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class PlanTourPopupViewModel
    /// </summary>
    [TestClass]
    public class PlanTourPopupViewModelTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up

            // run
            var viewModel = new PlanTourPopupViewModel(
                new PlanTourParameters
                {
                    WaypointIdList =
                    [
                        "wheretofly-path-rotwand",
                    ],
                },
                () => Task.CompletedTask);

            Assert.IsTrue(
                viewModel.WaitForPropertyChange(
                    nameof(viewModel.PlanTourList),
                    TimeSpan.FromSeconds(10)),
                "waiting for property change must succeed");

            // check
            Assert.IsTrue(viewModel.PlanTourList.Any(), "plan tour list must contain entries");
            Assert.IsNotNull(viewModel.PlanTourCommand, "command must have been initialized");
            Assert.IsNotNull(viewModel.CloseCommand, "command must have been initialized");
            Assert.IsFalse(viewModel.IsTourPlanningPossible, "tour planning must not be possible with 1 entry");
            Assert.IsTrue(viewModel.ShowWarningForMoreLocations, "warning for more entries must be shown");
        }
    }
}
