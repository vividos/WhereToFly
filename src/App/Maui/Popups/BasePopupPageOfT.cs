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
