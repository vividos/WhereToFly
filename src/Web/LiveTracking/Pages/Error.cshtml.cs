using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace WhereToFly.Web.LiveTracking.Pages
{
    /// <summary>
    /// Error page model
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// Request ID
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Indicates if request ID should be shown
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);

        /// <summary>
        /// Default GET handler
        /// </summary>
        public void OnGet()
        {
            this.RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier;
        }
    }
}
