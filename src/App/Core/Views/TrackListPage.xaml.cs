using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display track list; a single location entry can be tapped to get more infos.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TrackListPage : ContentPage
    {
        /// <summary>
        /// Creates a new track list page
        /// </summary>
        public TrackListPage()
        {
            this.Title = "Track list";

            this.InitializeComponent();

            this.BindingContext = new TrackListViewModel();
        }

        /// <summary>
        /// Called when an item was tapped on the track list
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnItemTapped_TrackListView(object sender, ItemTappedEventArgs args)
        {
            var localViewModel = this.BindingContext as TrackListViewModel;

            var trackListEntryViewModel = args.Item as TrackListEntryViewModel;
            localViewModel.ItemTappedCommand.Execute(trackListEntryViewModel.Track);
        }
    }
}
