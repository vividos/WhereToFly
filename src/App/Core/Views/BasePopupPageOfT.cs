using Rg.Plugins.Popup.Extensions;
using System.Threading.Tasks;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Base class for all popup pages that want to return a result.
    /// </summary>
    /// <typeparam name="TResult">type of the result</typeparam>
    public class BasePopupPage<TResult> : BasePopupPage, IPageResult<TResult>
        where TResult : class
    {
        /// <summary>
        /// Task completion source for result
        /// </summary>
        private readonly TaskCompletionSource<TResult> tcs = new();

        /// <summary>
        /// Task that resolves to the result object when the popup is closed.
        /// </summary>
        public Task<TResult> ResultTask => this.tcs.Task;

        /// <summary>
        /// Creates a new base popup page
        /// </summary>
        public BasePopupPage()
        {
            this.CloseWhenBackgroundIsClicked = true;
            this.tcs.Task.ContinueWith(
                async (_) => await this.Navigation.PopPopupAsync());
        }

        /// <summary>
        /// Called when user clicked on the background, dismissing the popup page.
        /// </summary>
        /// <returns>whatever the base class returns</returns>
        protected override bool OnBackgroundClicked()
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(default);
            }

            return base.OnBackgroundClicked();
        }

        /// <summary>
        /// Called when user naviaged back with the back button, dismissing the popup page.
        /// </summary>
        /// <returns>whatever the base class returns</returns>
        protected override bool OnBackButtonPressed()
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(default);
            }

            return base.OnBackButtonPressed();
        }

        /// <summary>
        /// Sets result value for popup page
        /// </summary>
        /// <param name="result">result value</param>
        protected void SetResult(TResult result)
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(result);
            }
        }
    }
}
