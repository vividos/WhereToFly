using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web view source factory implementation for .NET Standard
    /// </summary>
    internal partial class WebViewSourceFactory
    {
        /// <summary>
        /// Throws NotImplementedException
        /// </summary>
        /// <returns>thrown exception</returns>
        public Task<WebViewSource> PlatformGetMapViewSource()
            => throw new NotImplementedException("not implemented in netstandard project");

        /// <summary>
        /// Throws NotImplementedException
        /// </summary>
        /// <returns>thrown exception</returns>
        public Task<WebViewSource> PlatformGetHeightProfileViewSource()
            => throw new NotImplementedException("not implemented in netstandard project");
    }
}
