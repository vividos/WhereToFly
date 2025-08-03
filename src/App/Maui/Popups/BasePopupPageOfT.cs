using CommunityToolkit.Maui.Views;

namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Base class for all popup pages that want to return a result.
    /// </summary>
    /// <typeparam name="TResult">type of the result</typeparam>
    public class BasePopupPage<TResult> : Popup<TResult>
        where TResult : class?
    {
        /// <summary>
        /// Creates a new base popup page
        /// </summary>
        public BasePopupPage()
        {
            this.CanBeDismissedByTappingOutsideOfPopup = true;

            // Workaround: CommunityToolkit.Maui 12.1.0 Popups don't pick up styles defined in
            // Styles.xaml, so set them here; can be removed as soon as this bug is fixed:
            // https://github.com/CommunityToolkit/Maui/issues/2747
            this.Margin = 0;
            this.SetAppThemeColor(
                BackgroundColorProperty,
                Color.FromArgb("#F5F5F5"),
                Color.FromArgb("#606164"));
        }

        /// <summary>
        /// Sets result value for popup page and closes the popup
        /// </summary>
        /// <param name="result">result value</param>
        protected void SetResult(TResult? result)
        {
            if (result == null)
            {
                this.CloseAsync();
            }
            else
            {
                this.CloseAsync(result);
            }
        }
    }
}
