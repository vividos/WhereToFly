﻿using Rg.Plugins.Popup.Extensions;
using System.Threading.Tasks;
using WhereToFly.App.ViewModels;
using WhereToFly.Shared.Model;

namespace WhereToFly.App.Views
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
            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();

            this.BindingContext = new PlanTourPopupViewModel(
                planTourParameters,
                this.CloseAsync);
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
    }
}
