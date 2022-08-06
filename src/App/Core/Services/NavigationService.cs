using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Views;
using WhereToFly.Geo.Model;
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
            new()
            {
                { PageKey.MapPage, (typeof(MapPage), null) },
                { PageKey.LayerListPage, (typeof(LayerListPage), null) },
                { PageKey.LayerDetailsPage, (typeof(LayerDetailsPage), typeof(Layer)) },
                { PageKey.CurrentPositionDetailsPage, (typeof(CurrentPositionTabbedPage), null) },
                { PageKey.LocationListPage, (typeof(LocationListPage), null) },
                { PageKey.LocationDetailsPage, (typeof(LocationDetailsPage), typeof(Geo.Model.Location)) },
                { PageKey.EditLocationDetailsPage, (typeof(EditLocationDetailsPage), typeof(Geo.Model.Location)) },
                { PageKey.TrackListPage, (typeof(TrackListPage), null) },
                { PageKey.TrackInfoPage, (typeof(TrackInfoTabbedPage), typeof(Geo.Model.Track)) },
                { PageKey.TrackHeightProfilePage, (typeof(TrackHeightProfilePage), typeof(Geo.Model.Track)) },
                { PageKey.WeatherDashboardPage, (typeof(WeatherDashboardPage), null) },
                { PageKey.WeatherDetailsPage, (typeof(WeatherDetailsPage), typeof(WeatherIconDescription)) },
                { PageKey.SettingsPage, (typeof(SettingsPage), null) },
                { PageKey.InfoPage, (typeof(InfoPage), null) },
            };

        /// <summary>
        /// Mapping from popup page key to page type and, optionally, the type of parameter that
        /// must be passed.
        /// </summary>
        private static readonly Dictionary<PopupPageKey, (Type, Type)> PopupPageKeyToPageMap =
            new()
            {
                { PopupPageKey.AddLayerPopupPage, (typeof(AddLayerPopupPage), null) },
                { PopupPageKey.SelectWeatherIconPopupPage, (typeof(SelectWeatherIconPopupPage), typeof(string)) },
                { PopupPageKey.SetCompassTargetDirectionPopupPage, (typeof(SetCompassTargetDirectionPopupPage), null) },
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
        /// Navigates to the map page
        /// </summary>
        /// <returns>task to wait on</returns>
        public static Task GoToMap()
        {
            return Instance.NavigateAsync(PageKey.MapPage, animated: true);
        }

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
        /// Navigates to popup page and returns the popup page's result
        /// </summary>
        /// <typeparam name="TResult">type of result</typeparam>
        /// <param name="popupPageKey">popup page key</param>
        /// <param name="animated">indicates if popup page navigation should be animated</param>
        /// <param name="parameter">parameter; mandatory for some popup pages</param>
        /// <returns>result object</returns>
        public async Task<TResult> NavigateToPopupPageAsync<TResult>(
            PopupPageKey popupPageKey,
            bool animated,
            object parameter = null)
            where TResult : class
        {
            Debug.Assert(this.NavigationPage != null, "NavigationPage property must have been set");

            if (!MainThread.IsMainThread)
            {
                return await MainThread.InvokeOnMainThreadAsync(
                    async () => await this.NavigateToPopupPageAsync<TResult>(popupPageKey, animated, parameter));
            }

            if (!PopupPageKeyToPageMap.TryGetValue(popupPageKey, out (Type, Type) typeTuple))
            {
                throw new ArgumentException(
                    nameof(popupPageKey),
                    $"invalid popup page key: {popupPageKey}");
            }

            Type parameterType = typeTuple.Item2;

            if (parameterType != null &&
                parameter != null &&
                !parameterType.Equals(parameter.GetType()))
            {
                throw new ArgumentException(
                    nameof(parameter),
                    $"expected parameter of type {parameterType.FullName}, but type {parameter.GetType().FullName} was passed!");
            }

            Type popupPageType = typeTuple.Item1;

            PopupPage popupPage = null;
            if (parameter == null)
            {
                popupPage = (PopupPage)Activator.CreateInstance(popupPageType);
            }
            else
            {
                popupPage = (PopupPage)Activator.CreateInstance(popupPageType, parameter);
            }

            if (popupPage != null)
            {
                await popupPage.Navigation.PushPopupAsync(popupPage, animated);
            }
            else
            {
                throw new InvalidOperationException("couldn't create popup page for popup page key " + popupPageKey);
            }

            if (popupPage is IPageResult<TResult> pageResult)
            {
                return await pageResult.ResultTask;
            }
            else
            {
                Debug.Assert(
                    false,
                    $"the page's {popupPage.GetType().FullName} result type doesn't match the calling NavigateToPopupPageAsync result type {typeof(TResult).FullName}");
            }

            return null;
        }

        /// <summary>
        /// Navigates back in navigation stack
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task GoBack()
        {
            Debug.Assert(this.NavigationPage != null, "NavigationPage property must have been set");

            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(
                    async () => await this.NavigationPage.Navigation.PopAsync());
            }
            else
            {
                await this.NavigationPage.Navigation.PopAsync();
            }
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
