using System;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to select a weather icon to add to the weather dashboard
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectWeatherIconPage : ContentPage
    {
        /// <summary>
        /// View model for this page
        /// </summary>
        private SelectWeatherIconViewModel viewModel;

        /// <summary>
        /// Creates a new page object
        /// </summary>
        /// <param name="selectAction">action to call when a weather icon description was selected</param>
        public SelectWeatherIconPage(Action<WeatherIconDescription> selectAction)
        {
            this.Title = "Select a weather icon";
            this.BindingContext = this.viewModel = new SelectWeatherIconViewModel(selectAction);

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
    }
}
