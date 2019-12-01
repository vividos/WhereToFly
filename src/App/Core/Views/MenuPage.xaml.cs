using WhereToFly.App.Core.Services;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Page to display menu for the master-detail root page
    /// </summary>
    public partial class MenuPage : ContentPage
    {
        /// <summary>
        /// Creates new menu page
        /// </summary>
        public MenuPage()
        {
            this.InitializeComponent();

            this.BindingContext = new MenuViewModel();

            this.listView.ItemSelected += this.OnMenuItemSelected;
        }

        /// <summary>
        /// Called when a menu item has been selected; navigates to a new page
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnMenuItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var menuItemViewModel = args.SelectedItem as MenuViewModel.MenuItemViewModel;

            if (menuItemViewModel == null)
            {
                return;
            }

            this.listView.SelectedItem = null;

            string pageKey = menuItemViewModel.PageKey;

            App.RunOnUiThread(async () => await NavigationService.Instance.NavigateAsync(pageKey, true));
        }
    }
}
