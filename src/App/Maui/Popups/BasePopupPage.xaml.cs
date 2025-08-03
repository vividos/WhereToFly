using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Base class for all popup pages.
    /// </summary>
    public partial class BasePopupPage : Popup
    {
        /// <summary>
        /// Creates a popup object
        /// </summary>
        public BasePopupPage()
        {
            // Workaround: CommunityToolkit.Maui 12.1.0 Popups don't pick up styles defined in
            // Styles.xaml, so set them here; can be removed as soon as this bug is fixed:
            // https://github.com/CommunityToolkit/Maui/issues/2747
            this.Margin = 0;
            this.SetAppThemeColor(
                BackgroundColorProperty,
                Color.FromArgb("#F5F5F5"),
                Color.FromArgb("#606164"));
        }
    }
}
