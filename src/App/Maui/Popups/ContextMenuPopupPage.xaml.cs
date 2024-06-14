using WhereToFly.App.ViewModels;

namespace WhereToFly.App.Popups
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
            this.BindingContext = viewModel;
            this.InitializeComponent();
        }
    }
}
