using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Shared.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page to show all locations selected for tour planning. Also lets user start planning
    /// tour.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlanTourPopupPage : PopupPage
    {
        /// <summary>
        /// Creates a new popup page object
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        public PlanTourPopupPage(PlanTourParameters planTourParameters)
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();

            this.BindingContext = new PlanTourPopupViewModel(planTourParameters, this.ClosePopupPage);
        }

        /// <summary>
        /// Shows "Plan tour" popup page and lets the user edit the tour location list.
        /// </summary>
        /// <param name="planTourParameters">tour planning parameters</param>
        /// <returns>task to wait for</returns>
        public static async Task ShowAsync(PlanTourParameters planTourParameters)
        {
            var popupPage = new PlanTourPopupPage(planTourParameters);

            await popupPage.Navigation.PushPopupAsync(popupPage);
        }

        /// <summary>
        /// Closes popup page
        /// </summary>
        /// <returns>task to wait for</returns>
        private async Task ClosePopupPage()
        {
            await this.Navigation.PopPopupAsync();
        }
    }
}
