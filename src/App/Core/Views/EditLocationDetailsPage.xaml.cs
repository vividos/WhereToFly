using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to edit location details, such as name, type, internet link and description text.
    /// </summary>
    public partial class EditLocationDetailsPage : ContentPage
    {
        /// <summary>
        /// View model for page
        /// </summary>
        private readonly EditLocationDetailsViewModel viewModel;

        /// <summary>
        /// Creates new edit location details page
        /// </summary>
        /// <param name="location">location object to edit</param>
        public EditLocationDetailsPage(Location location)
        {
            this.Title = "Edit location";

            this.InitializeComponent();

            this.BindingContext = this.viewModel = new EditLocationDetailsViewModel(App.Settings, location);
        }

        #region Page lifecycle methods
        /// <summary>
        /// Called when the page is appearing
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            this.nameEntry.Focus();
        }

        /// <summary>
        /// Called when the page is about to disappear
        /// </summary>
        protected override void OnDisappearing()
        {
            Task.Run(async () => await this.viewModel.SaveChangesAsync());

            base.OnDisappearing();
        }
        #endregion
    }
}
