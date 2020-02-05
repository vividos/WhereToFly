using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Shows an info page displaying a CarouselView with the info (sub) pages.
    /// </summary>
    public partial class InfoPage : ContentPage
    {
        /// <summary>
        /// Creates a new info page
        /// </summary>
        public InfoPage()
        {
            this.BindingContext = new InfoPageViewModel();
            this.InitializeComponent();
        }
    }
}
