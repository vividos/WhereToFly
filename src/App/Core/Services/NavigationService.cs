using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.App.Core.Views;
using WhereToFly.App.Model;
using WhereToFly.Shared.Model;
using Xamarin.Essentials;
using Xamarin.Forms;

#pragma warning disable S4136 // All 'NavigateAsync' method overloads should be adjacent.

namespace WhereToFly.App.Core.Services
{
    /// <summary>
    /// Navigation service for the app; manages navigating to pages from any place in the app.
    /// </summary>
    public class NavigationService
    {
        /// <summary>
        /// Returns current instance of navigation service, registered with the DependencyService.
        /// </summary>
        public static NavigationService Instance
        {
            get { return DependencyService.Get<NavigationService>(); }
        }

        /// <summary>
        /// Navigation page to use to navigate between pages
        /// </summary>
        public NavigationPage NavigationPage { get; internal set; }

        /// <summary>
        /// Navigates to a page with given page key and parameters; async version
        /// </summary>
        /// <param name="pageKey">page key of page to navigate to</param>
        /// <param name="animated">indicates if page navigation should be animated</param>
        /// <param name="parameter">parameter object to pass; may be null</param>
        /// <returns>task to wait on</returns>
        public async Task NavigateAsync(string pageKey, bool animated, object parameter = null)
        {
            Type pageType = GetPageTypeFromPageKey(pageKey, parameter);

            if (pageType != null)
            {
                await this.NavigateAsync(pageType, animated, parameter);
            }
        }

        /// <summary>
        /// Returns Xamarin.Forms page type from given page key; also checks if needed parameters
        /// were passed.
        /// </summary>
        /// <param name="pageKey">page key</param>
        /// <param name="parameter">parameter; mandatory for some pages</param>
        /// <returns>page type</returns>
        private static Type GetPageTypeFromPageKey(string pageKey, object parameter)
        {
            Type pageType = null;

            switch (pageKey)
            {
                case Constants.PageKeyMapPage:
                    pageType = typeof(MapPage);
                    break;

                case Constants.PageKeyLayerListPage:
                    pageType = typeof(LayerListPage);
                    break;

                case Constants.PageKeyLayerDetailsPage:
                    Debug.Assert(
                        parameter is Layer,
                        "layer must have been passed as parameter");
                    pageType = typeof(LayerDetailsPage);
                    break;

                case Constants.PageKeyCurrentPositionDetailsPage:
                    pageType = typeof(CurrentPositionDetailsPage);
                    break;

                case Constants.PageKeyLocationListPage:
                    pageType = typeof(LocationListPage);
                    break;

                case Constants.PageKeyLocationDetailsPage:
                    Debug.Assert(
                        parameter is Model.Location,
                        "location must have been passed as parameter");
                    pageType = typeof(LocationDetailsPage);
                    break;

                case Constants.PageKeyEditLocationDetailsPage:
                    Debug.Assert(
                        parameter is Model.Location,
                        "location must have been passed as parameter");
                    pageType = typeof(EditLocationDetailsPage);
                    break;

                case Constants.PageKeyTrackListPage:
                    pageType = typeof(TrackListPage);
                    break;

                case Constants.PageKeyTrackInfoPage:
                    Debug.Assert(
                        parameter is Geo.Track,
                        "track must have been passed as parameter");
                    pageType = typeof(TrackInfoTabbedPage);
                    break;

                case Constants.PageKeyTrackHeightProfilePage:
                    Debug.Assert(
                        parameter is Geo.Track,
                        "track must have been passed as parameter");
                    pageType = typeof(TrackHeightProfilePage);
                    break;

                case Constants.PageKeyWeatherDashboardPage:
                    pageType = typeof(WeatherDashboardPage);
                    break;

                case Constants.PageKeyWeatherDetailsPage:
                    Debug.Assert(
                        parameter is WeatherIconDescription,
                        "weather icon description must have been passed as parameter");
                    pageType = typeof(WeatherDetailsPage);
                    break;

                case Constants.PageKeySelectWeatherIconPage:
                    Debug.Assert(
                        parameter is TaskCompletionSource<WeatherIconDescription>,
                        "task completion source must have been passed as parameter");
                    pageType = typeof(SelectWeatherIconPage);
                    break;

                case Constants.PageKeySettingsPage:
                    pageType = typeof(SettingsPage);
                    break;

                case Constants.PageKeyInfoPage:
                    pageType = typeof(InfoPage);
                    break;

                default:
                    Debug.Assert(false, "invalid page key");
                    break;
            }

            return pageType;
        }

        /// <summary>
        /// Navigates back in navigation stack
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task GoBack()
        {
            Debug.Assert(this.NavigationPage != null, "NavigationPage property must have been set");

            await this.NavigationPage.Navigation.PopAsync();
        }

        /// <summary>
        /// Navigates to a page with given type and parameters; async version
        /// </summary>
        /// <param name="pageType">page type</param>
        /// <param name="animated">indicates if page navigation should be animated</param>
        /// <param name="parameter">parameter object to pass; may be null</param>
        /// <returns>task to wait on</returns>
        public async Task NavigateAsync(Type pageType, bool animated, object parameter = null)
        {
            Debug.Assert(this.NavigationPage != null, "NavigationPage property must have been set");

            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(async () => await this.NavigateAsync(pageType, animated, parameter));
                return;
            }

            // close flyout if necessary
            if (App.Current.MainPage is FlyoutPage flyoutPage &&
                flyoutPage.IsPresented)
            {
                flyoutPage.IsPresented = false;
            }

            Page displayPage = null;
            if (pageType == typeof(MapPage))
            {
                await this.NavigationPage.Navigation.PopToRootAsync();
            }
            else if (parameter == null)
            {
                displayPage = (Page)Activator.CreateInstance(pageType);
            }
            else
            {
                displayPage = (Page)Activator.CreateInstance(pageType, parameter);
            }

            if (displayPage != null)
            {
                await this.NavigationPage.PushAsync(displayPage, animated);
            }
        }
    }
}
