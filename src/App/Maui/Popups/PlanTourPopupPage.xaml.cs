using WhereToFly.App.ViewModels;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Popup page to show all locations selected for tour planning. Also lets user start planning
    /// tour.
    /// </summary>
    public partial class PlanTourPopupPage : BasePopupPage
    {
        /// <summary>
        /// Creates a new popup page object
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        public PlanTourPopupPage(PlanTourParameters planTourParameters)
        {
            this.InitializeComponent();

            this.BindingContext = new PlanTourPopupViewModel(
                planTourParameters,
                () => this.CloseAsync());
        }
    }
}
