using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
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
            this.Title = "General";
            this.IconImageSource = new FileImageSource
            {
                File = Converter.ImagePathConverter.GetDeviceDependentImage("settings_outline"),
            };

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
