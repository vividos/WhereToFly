namespace WhereToFly.App.Services
{
    /// <summary>
    /// Interface to the navigation service for the app.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to a page with given page key and parameters
        /// </summary>
        /// <param name="pageKey">page key of page to navigate to</param>
        /// <param name="animated">indicates if page navigation should be animated</param>
        /// <param name="parameter">parameter object to pass; may be null</param>
        /// <returns>task to wait on</returns>
        Task NavigateAsync(PageKey pageKey, bool animated = true, object? parameter = null);

        /// <summary>
        /// Navigates to popup page and returns the popup page's result
        /// </summary>
        /// <typeparam name="TResult">type of result</typeparam>
        /// <param name="popupPageKey">popup page key</param>
        /// <param name="animated">indicates if popup page navigation should be animated</param>
        /// <param name="parameter">parameter; mandatory for some popup pages</param>
        /// <returns>result object</returns>
        Task<TResult?> NavigateToPopupPageAsync<TResult>(
            PopupPageKey popupPageKey,
            bool animated = true,
            object? parameter = null)
            where TResult : class?;

        /// <summary>
        /// Navigates back in navigation stack
        /// </summary>
        /// <returns>task to wait on</returns>
        Task GoBack();
    }
}
