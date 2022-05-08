namespace WhereToFly.App.MapView
{
    /// <summary>
    /// Factory for WebViewService instances
    /// </summary>
    internal static class WebViewSourceFactory
    {
        /// <summary>
        /// Default factory implementation
        /// </summary>
        private static IWebViewSourceFactory defaultImplementation;

        /// <summary>
        /// Returns the default web view source factory
        /// </summary>
        public static IWebViewSourceFactory Default
            => defaultImplementation ??= new WebViewSourceFactoryImpl();
    }
}
