using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Airspace;

namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Popup page for selecting airspace classes to import.
    /// </summary>
    public partial class SelectAirspaceClassPopupPage : BasePopupPage<ISet<AirspaceClass>>
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly SelectAirspaceClassPopupViewModel viewModel;

        /// <summary>
        /// Creates a new popup page to select airspace classes
        /// </summary>
        /// <param name="airspaceClassesList">airspace classes to choose from</param>
        public SelectAirspaceClassPopupPage(List<AirspaceClass> airspaceClassesList)
        {
            this.BindingContext = this.viewModel =
                new SelectAirspaceClassPopupViewModel(airspaceClassesList);

            this.InitializeComponent();
        }

        /// <summary>
        /// Called when user clicked on the "Filter airspaces" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedFilterAirspacesButton(object? sender, EventArgs args)
        {
            var airspaceClassesSet = this.viewModel.GetSelectedAirspaceClasses();

            this.SetResult(airspaceClassesSet);
        }
    }
}
