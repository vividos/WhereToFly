using System.Threading.Tasks;

namespace WhereToFly.App.Core
{
    /// <summary>
    /// Interface for pages and popup pages that wish to return a result
    /// </summary>
    /// <typeparam name="TResult">type of result to return</typeparam>
    public interface IPageResult<TResult>
        where TResult : class
    {
        /// <summary>
        /// Returns task that returns a result when awaited; when the page is cancelled, may
        /// return null.
        /// </summary>
        Task<TResult> ResultTask { get; }
    }
}
