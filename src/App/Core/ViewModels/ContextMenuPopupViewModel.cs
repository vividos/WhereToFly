using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for context menu popup page
    /// </summary>
    public class ContextMenuPopupViewModel : ObservableObject
    {
        /// <summary>
        /// Action to dismiss the popup page
        /// </summary>
        private readonly Action actionDismiss;

        #region Binding properties
        /// <summary>
        /// Caption text for context menu
        /// </summary>
        public string Caption { get; }

        /// <summary>
        /// List of context menu items to display
        /// </summary>
        public IList<MenuItem> ContextMenuItems { get; }

        /// <summary>
        /// Height of context menu list, depending on the number of items
        /// </summary>
        public double ContextMenuListHeight
            => this.ContextMenuItems.Count * 40;

        /// <summary>
        /// Currently selected context menu item
        /// </summary>
        public MenuItem SelectedMenuItem { get; set; }

        /// <summary>
        /// Command to execute when a menu item was selected
        /// </summary>
        public ICommand MenuItemSelectedCommand { get; }
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
            this.ContextMenuItems = contextMenuItems;
            this.actionDismiss = actionDismiss;

            this.MenuItemSelectedCommand = new Command(this.OnSelectedMenuItem);
        }

        /// <summary>
        /// Called when the user taps on a context menu item
        /// </summary>
        private void OnSelectedMenuItem()
        {
            if (this.SelectedMenuItem == null)
            {
                return;
            }

            this.actionDismiss.Invoke();

            this.SelectedMenuItem = null;
            this.OnPropertyChanged(nameof(this.SelectedMenuItem));
        }
    }
}
