using Rg.Plugins.Popup.Extensions;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for "select weather icon" function.
    /// </summary>
    public partial class SelectWeatherIconPopupPage : BasePopupPage
    {
        /// <summary>
        /// Task completion source to report back selected weather icon description.
        /// </summary>
        private readonly TaskCompletionSource<WeatherIconDescription> tcs = new TaskCompletionSource<WeatherIconDescription>();

        /// <summary>
        /// Weather icon group to filter list by; may be null to show all groups
        /// </summary>
        private readonly string group;

        /// <summary>
        /// View model for this popup page
        /// </summary>
        private SelectWeatherIconViewModel viewModel;

        /// <summary>
        /// Shows "Select weather web link" popup page, lets the user choose a weather icon
        /// description and returns it.
        /// </summary>
        /// <param name="group">weather icon group to filter by; may be null to show all groups</param>
        /// <returns>selected weather icon description, or null when user canceled the popup dialog</returns>
        public static async Task<WeatherIconDescription> ShowAsync(string group)
        {
            var popupPage = new SelectWeatherIconPopupPage(group);

            await popupPage.Navigation.PushPopupAsync(popupPage);

            var weatherIcon = await popupPage.tcs.Task;

            await popupPage.Navigation.PopPopupAsync(true);

            return weatherIcon;
        }

        /// <summary>
        /// Creates a new web link selection popup page
        /// </summary>
        /// <param name="group">weather icon group to filter by; may be null to show all groups</param>
        public SelectWeatherIconPopupPage(string group)
        {
            this.group = group;

            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            this.BindingContext = this.viewModel =
                new SelectWeatherIconViewModel(this.tcs, this.group);
        }

        /// <summary>
        /// Called when an item was tapped on the location list
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnItemTapped_WeatherIconListView(object sender, ItemTappedEventArgs args)
        {
            var weatherIconListEntryViewModel = args.Item as WeatherIconListEntryViewModel;
            this.viewModel.ItemTappedCommand.Execute(weatherIconListEntryViewModel.IconDescription);
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
    }
}
