using Microsoft.Maui.Handlers;

namespace WhereToFly.App.Controls
{
    /// <summary>
    /// Web view that shows the selected weather page. The web view's platform dependent renderer
    /// allows to long-tap any image and save it.
    /// </summary>
    public class WeatherWebView : WebView
    {
        /// <summary>
        /// Configures handlers for weather web view
        /// </summary>
        /// <param name="builder">app builder</param>
        /// <returns>app builder to chain calls</returns>
        public static MauiAppBuilder UseWeatherWebView(MauiAppBuilder builder)
        {
            return builder
                .ConfigureMauiHandlers((handlers) =>
                {
#if ANDROID
                    // Note: Using AppendToMapping() instead of ModifyMapping(), since this would
                    // interfere with other handlers, despite suggested here:
                    // https://github.com/dotnet/maui/pull/16032
                    WebViewHandler.Mapper.AppendToMapping(
                        "WeatherWebView-" + nameof(global::Android.Webkit.WebViewClient),
                        (handler, view) =>
                        {
                            if (handler is WebViewHandler mauiHandler &&
                                view is WeatherWebView)
                            {
                                handler.PlatformView.SetWebViewClient(
                                    new WeatherWebViewClient(mauiHandler));
                            }
                        });
#endif
                });
        }
    }
}
