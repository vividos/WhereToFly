using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.App.Core.Views;
using WhereToFly.App.Model;
using Xamarin.Forms;

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
            Type pageType = null;
            switch (pageKey)
            {
                case Constants.PageKeyMapPage:
                    pageType = typeof(MapPage);
                    break;

                case Constants.PageKeyCurrentPositionDetailsPage:
                    pageType = typeof(CurrentPositionDetailsPage);
                    break;

                case Constants.PageKeyLocationListPage:
                    pageType = typeof(LocationListPage);
                    break;

                case Constants.PageKeyLocationDetailsPage:
                    Debug.Assert(
                        parameter != null && parameter is Model.Location,
                        "location must have been passed as parameter");
                    pageType = typeof(LocationDetailsPage);
                    break;

                case Constants.PageKeyEditLocationDetailsPage:
                    Debug.Assert(
                        parameter != null && parameter is Model.Location,
                        "location must have been passed as parameter");
                    pageType = typeof(EditLocationDetailsPage);
                    break;

                case Constants.PageKeyTrackListPage:
                    pageType = typeof(TrackListPage);
                    break;

                case Constants.PageKeyTrackDetailsPage:
                    Debug.Assert(
                        parameter != null && parameter is Geo.Track,
                        "track must have been passed as parameter");
                    pageType = typeof(TrackDetailsPage);
                    break;

                case Constants.PageKeyWeatherDashboardPage:
                    pageType = typeof(WeatherDashboardPage);
                    break;

                case Constants.PageKeyWeatherDetailsPage:
                    Debug.Assert(
                        parameter != null && parameter is WeatherIconDescription,
                        "weather icon description must have been passed as parameter");
                    pageType = typeof(WeatherDetailsPage);
                    break;

                case Constants.PageKeySelectWeatherIconPage:
                    Debug.Assert(
                        parameter != null && parameter is Action<Model.WeatherIconDescription>,
                        "action must have been passed as parameter");
                    pageType = typeof(SelectWeatherIconPage);
                    break;

                case Constants.PageKeySettingsPage:
                    pageType = typeof(SettingsPage);
                    break;

                case Constants.PageKeyImportLocationsPage:
                    pageType = typeof(ImportLocationsPage);
                    break;

                case Constants.PageKeyInfoPage:
                    pageType = typeof(InfoPage);
                    break;

                default:
                    Debug.Assert(false, "invalid page key");
                    break;
            }

            if (pageType != null)
            {
                Device.BeginInvokeOnMainThread(async () => await this.NavigateAsync(pageType, animated, parameter));
            }
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

            // close drawer if necessary
            if (App.Current.MainPage is MasterDetailPage masterDetailPage &&
                masterDetailPage.IsPresented)
            {
                masterDetailPage.IsPresented = false;
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
