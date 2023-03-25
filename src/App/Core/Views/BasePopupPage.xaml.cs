using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System.Threading.Tasks;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Base class for all popup pages.
    /// </summary>
    public partial class BasePopupPage : PopupPage
    {
        /// <summary>
        /// Closes popup
        /// </summary>
        /// <returns>task to wait on</returns>
        protected async Task ClosePopupAsync()
        {
            await this.Navigation.PopPopupAsync(animate: true);
        }
    }
}
