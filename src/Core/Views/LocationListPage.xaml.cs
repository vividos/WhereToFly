using System.Threading.Tasks;
using WhereToFly.Core.Services;
using WhereToFly.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.Core.Views
{
    /// <summary>
    /// Page to display location list; the list can be filtered by a filter text, and a single
    /// location entry can be tapped to get more infos.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationListPage : ContentPage
    {
        /// <summary>
        /// Creates a new location list page
        /// </summary>
        public LocationListPage()
        {
            this.Title = "Location list";

            this.InitializeComponent();

            this.BindingContext = new LocationListViewModel();

            this.SetupToolbar();
        }

        /// <summary>
        /// Sets up toolbar for this page
        /// </summary>
        private void SetupToolbar()
        {
            this.AddImportLocationsToolbarButton();
        }

        /// <summary>
        /// Adds "Import locations" toolbar button
        /// </summary>
        private void AddImportLocationsToolbarButton()
        {
            ToolbarItem importLocationsButton = new ToolbarItem(
                "Import locations",
                "playlist_plus.xml",
                async () => await this.OnClicked_ToolbarButtonImportLocations(),
                ToolbarItemOrder.Primary)
            {
                AutomationId = "ImportLocations"
            };

            this.ToolbarItems.Add(importLocationsButton);
        }

        /// <summary>
        /// Called when toolbar button "Import locations" was clicked
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OnClicked_ToolbarButtonImportLocations()
        {
            await NavigationService.Instance.NavigateAsync(Constants.PageKeyImportLocationsPage, animated: true);
        }
    }
}
