using CommunityToolkit.Mvvm.ComponentModel;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for context menu popup page
    /// </summary>
    public class ContextMenuPopupViewModel : ObservableObject
    {
        #region Binding properties
        /// <summary>
        /// Caption text for context menu
        /// </summary>
        public string Caption { get; }

        /// <summary>
        /// List of context menu items to display
        /// </summary>
        public IList<ContextMenuItemViewModel> ContextMenuItems { get; }

        /// <summary>
        /// Height of context menu list, depending on the number of items
        /// </summary>
        public double ContextMenuListHeight
            => this.ContextMenuItems.Count * 40;
        #endregion

        /// <summary>
        /// Creates a new view model for the context menu popup
        /// </summary>
        /// <param name="caption">context menu caption</param>
        /// <param name="contextMenuItems">list of context menu items</param>
        /// <param name="actionDismiss">action to dismiss the popup page</param>
        public ContextMenuPopupViewModel(
            string caption,
            IList<MenuItem> contextMenuItems,
            Action actionDismiss)
        {
            this.Caption = caption;
            this.ContextMenuItems =
                contextMenuItems.Select(
                    menuItem => new ContextMenuItemViewModel(menuItem, actionDismiss))
                .ToList();
        }
    }
}
