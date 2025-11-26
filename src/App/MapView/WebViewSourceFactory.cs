using Microsoft.Maui.Controls;

namespace WhereToFly.App.MapView;

/// <summary>
/// Factory for WebViewSource instances
/// </summary>
internal static class WebViewSourceFactory
{
    /// <summary>
    /// Base URL for WebLib library in WebView
    /// </summary>
#pragma warning disable S1075 // URIs should not be hardcoded
#if WINDOWS
    private static readonly string WebLibWebViewBaseUrl = "https://appdir/weblib/";
#elif ANDROID
    private static readonly string WebLibWebViewBaseUrl = "https://appassets.androidplatform.net/assets/weblib/";
#else
    private static readonly string WebLibWebViewBaseUrl = "https://weblib/";
#endif
#pragma warning restore S1075 // URIs should not be hardcoded

    /// <summary>
    /// Returns HTML web view source for <see cref="MapView"/>
    /// </summary>
    /// <returns>web view source</returns>
    public static WebViewSource GetMapViewSource()
    {
        return new UrlWebViewSource
        {
            Url = WebLibWebViewBaseUrl + "mapView.html",
        };
    }

    /// <summary>
    /// Returns HTML web view source for <see cref="HeightProfileView"/>
    /// </summary>
    /// <returns>web view source</returns>
    public static WebViewSource GetHeightProfileViewSource()
    {
        return new UrlWebViewSource
        {
            Url = WebLibWebViewBaseUrl + "heightProfileView.html",
        };
    }
}
