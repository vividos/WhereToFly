namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Base class for all popup pages that want to return a result.
    /// </summary>
    /// <typeparam name="TResult">type of the result</typeparam>
    public class BasePopupPage<TResult> : BasePopupPage, IPageResult<TResult>
        where TResult : class?
    {
        /// <summary>
        /// Task that resolves to the result object when the popup is closed.
        /// </summary>
        public Task<TResult?> ResultTask => this.GetResult();

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
        protected void SetResult(TResult result)
        {
            this.Close(result);
        }

        /// <summary>
        /// Retrieves the result of the popup page
        /// </summary>
        /// <returns>result object, or null when user cancelled the popup</returns>
        /// <exception cref="InvalidOperationException">
        /// thrown when a different type of object was returned as result
        /// </exception>
        private async Task<TResult?> GetResult()
        {
            object? result = await this.Result;
            if (result == null)
            {
                return null;
            }

            if (result is not TResult)
            {
                throw new InvalidOperationException(
                    $"Result task is not of type {typeof(TResult).FullName} " +
                    $"but instead of type {result.GetType().FullName}");
            }

            return result as TResult;
        }
    }
}
