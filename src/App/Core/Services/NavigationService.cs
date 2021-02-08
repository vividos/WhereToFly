using System;
using System.Collections.Generic;
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
        /// Mapping from page key to page type and, optionally, the type of parameter that must be
        /// passed
        /// </summary>
        private static readonly Dictionary<PageKey, (Type, Type)> PageKeyToPageMap =
            new Dictionary<PageKey, (Type, Type)>
            {
                { PageKey.MapPage, (typeof(MapPage), null) },
                { PageKey.LayerListPage, (typeof(LayerListPage), null) },
                { PageKey.LayerDetailsPage, (typeof(LayerDetailsPage), typeof(Layer)) },
                { PageKey.CurrentPositionDetailsPage, (typeof(CurrentPositionDetailsPage), null) },
                { PageKey.LocationListPage, (typeof(LocationListPage), null) },
                { PageKey.LocationDetailsPage, (typeof(LocationDetailsPage), typeof(Model.Location)) },
                { PageKey.EditLocationDetailsPage, (typeof(EditLocationDetailsPage), typeof(Model.Location)) },
                { PageKey.TrackListPage, (typeof(TrackListPage), null) },
                { PageKey.TrackInfoPage, (typeof(TrackInfoTabbedPage), typeof(Geo.Track)) },
                { PageKey.TrackHeightProfilePage, (typeof(TrackHeightProfilePage), typeof(Geo.Track)) },
                { PageKey.WeatherDashboardPage, (typeof(WeatherDashboardPage), null) },
                { PageKey.WeatherDetailsPage, (typeof(WeatherDetailsPage), typeof(WeatherIconDescription)) },
                {
                    PageKey.SelectWeatherIconPage,
                    (typeof(SelectWeatherIconPage), typeof(TaskCompletionSource<WeatherIconDescription>))
                },
                { PageKey.SettingsPage, (typeof(SettingsPage), null) },
                { PageKey.InfoPage, (typeof(InfoPage), null) },
            };

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
        public async Task NavigateAsync(PageKey pageKey, bool animated, object parameter = null)
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
        private static Type GetPageTypeFromPageKey(PageKey pageKey, object parameter)
        {
            Debug.Assert(
                PageKeyToPageMap.ContainsKey(pageKey),
                "page key couldn't be found");

            var tuple = PageKeyToPageMap[pageKey];
            Type pageType = tuple.Item1;

            Type parameterType = tuple.Item2;
            if (parameterType != null)
            {
                Debug.Assert(parameter != null, "passed parameter must be non-null");

                Debug.Assert(
                    parameterType.FullName == parameter.GetType().FullName,
                    "passed parameter must be of the correct type " + parameterType.Name);
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
