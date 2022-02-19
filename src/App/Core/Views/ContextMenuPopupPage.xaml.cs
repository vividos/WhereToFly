using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup to display context menu items list
    /// </summary>
    public partial class ContextMenuPopupPage : BasePopupPage
    {
        /// <summary>
        /// Creates a new context menu popup
        /// </summary>
        /// <param name="viewModel">view model for the popup</param>
        public ContextMenuPopupPage(ContextMenuPopupViewModel viewModel)
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.BindingContext = viewModel;
            this.InitializeComponent();
        }
    }
}
