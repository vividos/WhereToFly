using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for a single context menu item
    /// </summary>
    public class ContextMenuItemViewModel
    {
        /// <summary>
        /// Menu item to wrap
        /// </summary>
        private readonly MenuItem menuItem;

        #region Binding properties
        /// <summary>
        /// Image source for the menu item icon
        /// </summary>
        public ImageSource IconImageSource => this.menuItem.IconImageSource;

        /// <summary>
        /// Menu item text
        /// </summary>
        public string Text => this.menuItem.Text;

        /// <summary>
        /// Indicates if the menu item is enabled
        /// </summary>
        public bool IsEnabled => this.menuItem.IsEnabled;

        /// <summary>
        /// Command to execute when the menu item is tapped
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// Command parameter for the menu item
        /// </summary>
        public object CommandParameter => this.menuItem.CommandParameter;

        /// <summary>
        /// Foreground color for the menu item icon and text
        /// </summary>
        public Color ForegroundColor { get; }

        /// <summary>
        /// Background color for the menu item; red when it is a destructive operation
        /// </summary>
        public Color BackgroundColor { get; } = Color.Transparent;
        #endregion

        /// <summary>
        /// Creates a new view model for a single context menu item
        /// </summary>
        /// <param name="menuItem">menu item</param>
        /// <param name="actionDismiss">action to call to dismiss the context menu</param>
        public ContextMenuItemViewModel(MenuItem menuItem, Action actionDismiss)
        {
            this.menuItem = menuItem;

            string foregroundColorName = "LabelTextColor";
            string backgroundColorName = null;

            if (this.menuItem.IsDestructive)
            {
                foregroundColorName = "MenuItemDestructiveForegroundColor";
                backgroundColorName = "MenuItemDestructiveBackgroundColor";
            }

            if (!this.menuItem.IsEnabled)
            {
                foregroundColorName = "MenuItemDisabledForegroundColor";
                backgroundColorName = "MenuItemDisabledBackgroundColor";
            }

            this.ForegroundColor =
                Color.FromHex(App.GetResourceColor(foregroundColorName));

            if (backgroundColorName != null)
            {
                this.BackgroundColor =
                    Color.FromHex(App.GetResourceColor(backgroundColorName));
            }

            this.Command = new Command(
                () =>
                {
                    if (!this.menuItem.IsEnabled)
                    {
                        return;
                    }

                    actionDismiss?.Invoke();

                    this.menuItem.Command.Execute(this.menuItem.CommandParameter);
                });
        }
    }
}
