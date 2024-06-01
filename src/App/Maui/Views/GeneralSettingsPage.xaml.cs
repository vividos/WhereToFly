using WhereToFly.App.ViewModels;

namespace WhereToFly.App.Views
{
    /// <summary>
    /// Settings page to configure general app settings
    /// </summary>
    public partial class GeneralSettingsPage : ContentPage
    {
        /// <summary>
        /// View model for this page
        /// </summary>
        private readonly GeneralSettingsViewModel viewModel;

        /// <summary>
        /// Creates new general settings page
        /// </summary>
        public GeneralSettingsPage()
        {
            this.BindingContext = this.viewModel = new GeneralSettingsViewModel();

            this.InitializeComponent();
        }

        /// <summary>
        /// Called when page is about to disappear
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            Task.Run(this.viewModel.StoreDataAsync);
        }
    }
}
