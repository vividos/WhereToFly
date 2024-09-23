using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.App.Services;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Navigation service implementation for unit tests
    /// </summary>
    internal class UnitTestNavigationService : INavigationService
    {
        /// <summary>
        /// Page key of page where the service last navigated to
        /// </summary>
        public PageKey LastPageKey { get; private set; }

        /// <summary>
        /// Popup page key of popup page where the service last navigated to
        /// </summary>
        public PopupPageKey LastPopupPageKey { get; private set; }

        /// <summary>
        /// Stores the popup page result that the next call to
        /// <see cref="NavigateToPopupPageAsync"/> will return
        /// </summary>
        public object? PopupPageResult { get; set; }

        /// <summary>
        /// Number of times that <see cref="GoBack()"/> was called
        /// </summary>
        public int GoBackCount { get; private set; }

        /// <summary>
        /// Navigates to a page with given key; sets the <see cref="LastPageKey"/> property.
        /// </summary>
        /// <param name="pageKey">page key</param>
        /// <param name="animated">indicates if page navigation should be animated</param>
        /// <param name="parameter">page parameter</param>
        /// <returns>task to wait on</returns>
        public Task NavigateAsync(
            PageKey pageKey,
            bool animated = true,
            object? parameter = null)
        {
            Debug.WriteLine($"NavigationService: Navigating to page {pageKey}, with parameter {parameter}");

            this.LastPageKey = pageKey;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Navigates to a popup page with a key and returns a result object, stored in
        /// <see cref="PopupPageResult"/>; sets the <see cref="LastPopupPageKey"/> property.
        /// </summary>
        /// <typeparam name="TResult">type of the result object</typeparam>
        /// <param name="popupPageKey">popup page key</param>
        /// <param name="animated">indicates if page navigation should be animated</param>
        /// <param name="parameter">page parameter</param>
        /// <returns>result, or null when page result was null</returns>
        /// <exception cref="InvalidOperationException">
        /// thrown when the type TResult doesn't match <see cref="PopupPageResult"/>
        /// </exception>
        public Task<TResult?> NavigateToPopupPageAsync<TResult>(
            PopupPageKey popupPageKey,
            bool animated = true,
            object? parameter = null)
            where TResult : class?
        {
            Debug.WriteLine($"NavigationService: Navigating to popup page {popupPageKey}, with parameter {parameter}");

            if (this.PopupPageResult is not null and not TResult)
            {
                throw new InvalidOperationException(
                    $"tried to return popup result with type {this.PopupPageResult.GetType().Name}" +
                    $"but the caller expected type {typeof(TResult).Name}");
            }

            this.LastPopupPageKey = popupPageKey;

            object? result = this.PopupPageResult;
            this.PopupPageResult = null;

            return Task.FromResult(result as TResult);
        }

        /// <summary>
        /// Navigates back; number of calls to the method is counted in <see cref="GoBackCount"/>
        /// </summary>
        /// <returns>task to wait on</returns>
        public Task GoBack()
        {
            Debug.WriteLine("NavigationService: GoBack() was called");

            this.GoBackCount++;

            return Task.CompletedTask;
        }
    }
}
