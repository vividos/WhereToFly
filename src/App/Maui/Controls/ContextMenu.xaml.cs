﻿using CommunityToolkit.Maui.Views;
using System.Windows.Input;
using WhereToFly.App.Logic;
using WhereToFly.App.Popups;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.Controls
{
    /// <summary>
    /// Context menu; provides a 3-dot button that opens a popup with the context menu items when
    /// tapped.
    /// </summary>
    public partial class ContextMenu : ContentView
    {
        #region Binding properties
        /// <summary>
        /// Image source for the context menu 3-dot button
        /// </summary>
        public ImageSource ContextMenuImageSource { get; }

        /// <summary>
        /// Context menu command
        /// </summary>
        public ICommand ContextMenuCommand { get; }

        /// <summary>
        /// Caption text for the context menu
        /// </summary>
        public string Caption { get; set; } = string.Empty;

        /// <summary>
        /// Bindable property key for the read-only items property
        /// </summary>
        private static readonly BindablePropertyKey ItemsPropertyKey =
            BindableProperty.CreateReadOnly(
                propertyName: nameof(Items),
                returnType: typeof(MenuItemCollection),
                declaringType: typeof(ContextMenu),
                defaultValue: null,
                defaultValueCreator: bo => new MenuItemCollection());

        /// <summary>
        /// Binding property for the context menu items, specifying menu items to display in the
        /// context menu
        /// </summary>
        public static readonly BindableProperty ItemsProperty =
            ItemsPropertyKey.BindableProperty;

        /// <summary>
        /// List of context menu items to display
        /// </summary>
        public MenuItemCollection Items
            => (MenuItemCollection)this.GetValue(ItemsProperty);
        #endregion

        /// <summary>
        /// Creates a new context menu view
        /// </summary>
        public ContextMenu()
        {
            this.BindingContext = this;

            this.ContextMenuImageSource =
                SvgImageCache.GetImageSource("icons/dots-vertical.svg");

            this.ContextMenuCommand = new Command(this.ShowContextMenu);

            this.InitializeComponent();
        }

        /// <summary>
        /// Called when the parent element is set; used to wait on a binding context change
        /// </summary>
        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (this.Parent != null)
            {
                this.Parent.BindingContextChanged += this.OnParentBindingContextChanged;
            }
        }

        /// <summary>
        /// Called when the parent's binding context has changed. Sets the new binding context to
        /// all context menu items
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnParentBindingContextChanged(object? sender, EventArgs args)
        {
            this.Parent.BindingContextChanged -= this.OnParentBindingContextChanged;

            object bindingContext = this.Parent.BindingContext;
            for (int itemsIndex = 0; itemsIndex < this.Items.Count; itemsIndex++)
            {
                SetInheritedBindingContext(
                    this.Items[itemsIndex],
                    bindingContext);
            }
        }

        /// <summary>
        /// Shows context menu
        /// </summary>
        private void ShowContextMenu()
        {
            foreach (var item in this.Items)
            {
                item.IsEnabled &= item.Command?.CanExecute(item.CommandParameter) ?? false;
            }

            ContextMenuPopupPage? popupPage = null;

            var viewModel = new ContextMenuPopupViewModel(
                this.Caption,
                this.Items,
                () =>
                {
                    popupPage?.Close();
                });

            popupPage = new ContextMenuPopupPage(viewModel);
            UserInterface.MainPage.ShowPopup(popupPage);
        }
    }
}
