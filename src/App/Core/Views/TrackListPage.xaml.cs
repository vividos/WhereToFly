using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display track list; a single location entry can be tapped to get more infos.
    /// </summary>
    public partial class TrackListPage : ContentPage
    {
        /// <summary>
        /// View model for this page
        /// </summary>
        private readonly TrackListViewModel viewModel;

        /// <summary>
        /// Creates a new track list page
        /// </summary>
        public TrackListPage()
        {
            this.InitializeComponent();

            this.BindingContext = this.viewModel = new TrackListViewModel();
        }

        #region Page lifecycle methods
        /// <summary>
        /// Called when page is appearing; get current position
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            Task.Run(this.viewModel.CheckReloadNeeded);
        }
        #endregion
    }
}
