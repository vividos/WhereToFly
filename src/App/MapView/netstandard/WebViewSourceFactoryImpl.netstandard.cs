using System;
using Xamarin.Forms;

namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Web view source factory implementation for .NET Standard
    /// </summary>
    public partial class WebViewSourceFactoryImpl : IWebViewSourceFactory
    {
        /// <summary>
        /// Throws NotImplementedException
        /// </summary>
        /// <returns>thrown exception</returns>
        public WebViewSource GetMapViewSource()
            => throw new NotImplementedException("not implemented in netstandard project");

        /// <summary>
        /// Throws NotImplementedException
        /// </summary>
        /// <returns>thrown exception</returns>
        public WebViewSource GetHeightProfileViewSource()
            => throw new NotImplementedException("not implemented in netstandard project");
    }
}
