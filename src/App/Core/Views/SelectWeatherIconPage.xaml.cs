using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to select a weather icon to add to the weather dashboard
    /// </summary>
    public partial class SelectWeatherIconPage : ContentPage
    {
        /// <summary>
        /// Task completion source to use when a weather icon description was selected
        /// </summary>
        private readonly TaskCompletionSource<WeatherIconDescription> tcs;

        /// <summary>
        /// View model for this page
        /// </summary>
        private SelectWeatherIconViewModel viewModel;

        /// <summary>
        /// Creates a new page object
        /// </summary>
        /// <param name="tcs">
        /// task completion source to use when a weather icon description was selected
        /// </param>
        public SelectWeatherIconPage(TaskCompletionSource<WeatherIconDescription> tcs)
        {
            this.Title = "Select a weather icon";
            this.tcs = tcs;
            this.BindingContext = this.viewModel = new SelectWeatherIconViewModel(tcs);

            this.InitializeComponent();
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
        /// Called when the page is about to disappear; checks if the view model already set a
        /// task result.
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(null);
            }
        }
    }
}
