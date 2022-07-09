using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Services.SqliteDatabase;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Shared.Model;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class PlanTourPopupViewModel
    /// </summary>
    [TestClass]
    public class PlanTourPopupViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks and DependencyService with
        /// DataService and Platform.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IDataService, SqliteDatabaseDataService>();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
        }

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
                    WaypointIdList = new List<string>
                    {
                        "wheretofly-path-rotwand",
                    },
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
