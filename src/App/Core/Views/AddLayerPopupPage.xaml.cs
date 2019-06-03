using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Shared.Model;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for adding a new layer and edit its properties.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddLayerPopupPage : PopupPage
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly AddLayerPopupViewModel viewModel;

        /// <summary>
        /// Task completion source to report back edited layer
        /// </summary>
        private TaskCompletionSource<Layer> tcs;

        /// <summary>
        /// Creates a new popup page to edit layer properties
        /// </summary>
        /// <param name="layer">layer to edit</param>
        public AddLayerPopupPage(Layer layer)
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();

            this.BindingContext = this.viewModel = new AddLayerPopupViewModel(layer);
        }

        /// <summary>
        /// Shows "add layer" popup page and lets the user edit the layer properties.
        /// </summary>
        /// <param name="layer">layer to edit</param>
        /// <returns>edited layer, or null when user canceled the popup dialog</returns>
        public static async Task<Layer> ShowAsync(Layer layer)
        {
            var popupPage = new AddLayerPopupPage(layer)
            {
                tcs = new TaskCompletionSource<Layer>()
            };

            await popupPage.Navigation.PushPopupAsync(popupPage);

            return await popupPage.tcs.Task;
        }

        /// <summary>
        /// Called when user clicked on the background, dismissing the popup page.
        /// </summary>
        /// <returns>whatever the base class returns</returns>
        protected override bool OnBackgroundClicked()
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(null);
            }

            return base.OnBackgroundClicked();
        }

        /// <summary>
        /// Called when user naviaged back with the back button, dismissing the popup page.
        /// </summary>
        /// <returns>whatever the base class returns</returns>
        protected override bool OnBackButtonPressed()
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(null);
            }

            return base.OnBackButtonPressed();
        }

        /// <summary>
        /// Called when user clicked on the "Add layer" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClickedAddLayerButton(object sender, EventArgs args)
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(this.viewModel.Layer);
            }

            await this.Navigation.PopPopupAsync();
        }
    }
}
