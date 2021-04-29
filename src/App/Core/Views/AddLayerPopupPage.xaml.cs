using System;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for adding a new layer and edit its properties.
    /// </summary>
    public partial class AddLayerPopupPage : BasePopupPage<Layer>
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly AddLayerPopupViewModel viewModel;

        /// <summary>
        /// Creates a new popup page to edit layer properties
        /// </summary>
        /// <param name="layer">layer to edit</param>
        public AddLayerPopupPage(Layer layer)
        {
            this.InitializeComponent();

            this.BindingContext = this.viewModel = new AddLayerPopupViewModel(layer);
        }

        /// <summary>
        /// Called when user clicked on the "Add layer" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedAddLayerButton(object sender, EventArgs args)
        {
            this.SetResult(this.viewModel.Layer);
        }
    }
}
