using System;
using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for setting a compass target direction
    /// </summary>
    public partial class SetCompassTargetDirectionPopupPage : BasePopupPage<Tuple<int>>
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly SetCompassTargetDirectionPopupViewModel viewModel;

        /// <summary>
        /// Creates a new popup page to setting compass target direction
        /// </summary>
        /// <param name="initialDirection">initial direction to use</param>
        public SetCompassTargetDirectionPopupPage(int initialDirection)
        {
            this.InitializeComponent();

            this.BindingContext = this.viewModel =
                new SetCompassTargetDirectionPopupViewModel(initialDirection);
        }

        /// <summary>
        /// Called when user clicked on the "set direction" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedSetDirectionButton(object sender, EventArgs args)
        {
            this.SetResult(new Tuple<int>(this.viewModel.CompassDirection));
        }
    }
}
