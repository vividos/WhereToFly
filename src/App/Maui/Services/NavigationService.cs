using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using WhereToFly.App.MapView.Models;
using WhereToFly.App.Models;
using WhereToFly.App.Pages;
using WhereToFly.App.Popups;
using WhereToFly.Geo.Airspace;
using WhereToFly.Geo.Model;
using WhereToFly.Shared.Model;
using Location = WhereToFly.Geo.Model.Location;

#pragma warning disable S4136 // All 'NavigateAsync' method overloads should be adjacent.

namespace WhereToFly.App.Services
{
    /// <summary>
    /// Navigation service for the app; manages navigating to pages from any place in the app.
    /// </summary>
    public class NavigationService : INavigationService
    {
        /// <summary>
        /// Info about a page that can be creaetd by the navigation service
        /// </summary>
        /// <param name="PageType">page type; must derive from Page</param>
        /// <param name="ParameterType">parameter type; can be null</param>
        internal record struct PageInfo(
            [param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            Type PageType,
            Type? ParameterType);

        /// <summary>
        /// Info about a popup page that can be creaetd by the navigation service and has a return
        /// type
        /// </summary>
        /// <param name="PopupPageType">popup page type; must derive from Popup</param>
        /// <param name="ReturnType">return type; can be null</param>
        /// <param name="ParameterType">parameter type; can be null</param>
        internal record struct PopupPageInfo(
            [param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            Type PopupPageType,
            Type? ReturnType,
            Type? ParameterType);

        /// <summary>
        /// Mapping from page key to page type and, optionally, the type of parameter that must be
        /// passed
        /// </summary>
        private static readonly Dictionary<PageKey, PageInfo> PageKeyToPageMap =
            new()
            {
                { PageKey.MapPage, new PageInfo(typeof(MapPage), null) },
                { PageKey.LayerListPage, new PageInfo(typeof(LayerListPage), null) },
                { PageKey.LayerDetailsPage, new PageInfo(typeof(LayerDetailsPage), typeof(Layer)) },
                { PageKey.CurrentPositionDetailsPage, new PageInfo(typeof(CurrentPositionTabbedPage), null) },
                { PageKey.LocationListPage, new PageInfo(typeof(LocationListPage), null) },
                { PageKey.LocationDetailsPage, new PageInfo(typeof(LocationDetailsPage), typeof(Location)) },
                { PageKey.EditLocationDetailsPage, new PageInfo(typeof(EditLocationDetailsPage), typeof(Location)) },
                { PageKey.TrackListPage, new PageInfo(typeof(TrackListPage), null) },
                { PageKey.TrackInfoPage, new PageInfo(typeof(TrackInfoTabbedPage), typeof(Track)) },
                { PageKey.TrackHeightProfilePage, new PageInfo(typeof(TrackHeightProfilePage), typeof(Track)) },
                { PageKey.WeatherDashboardPage, new PageInfo(typeof(WeatherDashboardPage), null) },
                { PageKey.WeatherDetailsPage, new PageInfo(typeof(WeatherDetailsPage), typeof(WeatherIconDescription)) },
                { PageKey.SettingsPage, new PageInfo(typeof(SettingsPage), null) },
                { PageKey.InfoPage, new PageInfo(typeof(InfoPage), null) },
            };

        /// <summary>
        /// Mapping from popup page key to page type, the return type, and optionally, the type of
        /// parameter that must be passed.
        /// </summary>
        private static readonly Dictionary<PopupPageKey, PopupPageInfo> PopupPageKeyToPageMap =
            new()
            {
                { PopupPageKey.AddLayerPopupPage, new PopupPageInfo(typeof(AddLayerPopupPage), typeof(Layer), typeof(Layer)) },
                { PopupPageKey.AddLiveWaypointPopupPage, new PopupPageInfo(typeof(AddLiveWaypointPopupPage), typeof(Location), typeof(Location)) },
                { PopupPageKey.AddTrackPopupPage, new PopupPageInfo(typeof(AddTrackPopupPage), typeof(Track), typeof(Track)) },
                { PopupPageKey.AddWeatherLinkPopupPage, new PopupPageInfo(typeof(AddWeatherLinkPopupPage), typeof(WeatherIconDescription), null) },
                { PopupPageKey.FilterTakeoffDirectionsPopupPage, new PopupPageInfo(typeof(FilterTakeoffDirectionsPopupPage), typeof(LocationFilterSettings), typeof(LocationFilterSettings)) },
                { PopupPageKey.FindLocationPopupPage, new PopupPageInfo(typeof(FindLocationPopupPage), typeof(string), null) },
                { PopupPageKey.FlyingRangePopupPage, new PopupPageInfo(typeof(FlyingRangePopupPage), typeof(FlyingRangeParameters), null) },
                { PopupPageKey.PlanTourPopupPage, new PopupPageInfo(typeof(PlanTourPopupPage), null, typeof(PlanTourParameters)) },
                { PopupPageKey.SelectAirspaceClassPopupPage, new PopupPageInfo(typeof(SelectAirspaceClassPopupPage), typeof(ISet<AirspaceClass>), typeof(List<AirspaceClass>)) },
                { PopupPageKey.SelectWeatherIconPopupPage, new PopupPageInfo(typeof(SelectWeatherIconPopupPage), typeof(WeatherIconDescription), typeof(string)) },
                { PopupPageKey.SetCompassTargetDirectionPopupPage, new PopupPageInfo(typeof(SetCompassTargetDirectionPopupPage), typeof(Tuple<int>), null) },
                { PopupPageKey.SetTrackInfosPopupPage, new PopupPageInfo(typeof(SetTrackInfosPopupPage), typeof(Track), typeof(Track)) },
            };

        /// <summary>
        /// Default popup options
        /// </summary>
        public static readonly PopupOptions DefaultPopupOptions =
            new PopupOptions
            {
                PageOverlayColor = Colors.White.WithAlpha(0.5f),
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(8),
                    StrokeThickness = 0,
                },
                Shadow = null,
            };

        /// <summary>
        /// Navigation page to use to navigate between pages
        /// </summary>
        public NavigationPage? NavigationPage { get; internal set; }

        /// <summary>
        /// Navigates to the map page
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task GoToMap()
        {
            await this.NavigateAsync(PageKey.MapPage, animated: true);
        }

        /// <summary>
        /// Navigates to a page with given page key and parameters; async version
        /// </summary>
        /// <param name="pageKey">page key of page to navigate to</param>
        /// <param name="animated">indicates if page navigation should be animated</param>
        /// <param name="parameter">parameter object to pass; may be null</param>
        /// <returns>task to wait on</returns>
        public async Task NavigateAsync(PageKey pageKey, bool animated = true, object? parameter = null)
        {
            Type pageType = GetPageTypeFromPageKey(pageKey, parameter);

            if (pageType != null)
            {
                await this.NavigateAsync(pageType, animated, parameter);
            }
        }

        /// <summary>
        /// Returns content page type from given page key; also checks if needed parameters
        /// were passed.
        /// </summary>
        /// <param name="pageKey">page key</param>
        /// <param name="parameter">parameter; mandatory for some pages</param>
        /// <returns>page type</returns>
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        private static Type GetPageTypeFromPageKey(PageKey pageKey, object? parameter)
        {
            Debug.Assert(
                PageKeyToPageMap.ContainsKey(pageKey),
                "page key couldn't be found");

            var pageInfo = PageKeyToPageMap[pageKey];

            Type? parameterType = pageInfo.ParameterType;
            if (parameterType != null)
            {
                Debug.Assert(parameter != null, "passed parameter must be non-null");

                Debug.Assert(
                    parameterType.FullName == parameter!.GetType().FullName,
                    "passed parameter must be of the correct type " + parameterType.Name);
            }

            return pageInfo.PageType;
        }

        /// <summary>
        /// Navigates to popup page and returns the popup page's result
        /// </summary>
        /// <typeparam name="TResult">type of result</typeparam>
        /// <param name="popupPageKey">popup page key</param>
        /// <param name="animated">indicates if popup page navigation should be animated</param>
        /// <param name="parameter">parameter; mandatory for some popup pages</param>
        /// <returns>result object</returns>
        public async Task<TResult?> NavigateToPopupPageAsync<TResult>(
            PopupPageKey popupPageKey,
            bool animated = true,
            object? parameter = null)
            where TResult : class?
        {
            if (this.NavigationPage == null)
            {
                throw new InvalidOperationException("Navigation page wasn't initialized properly");
            }

            if (!MainThread.IsMainThread)
            {
                return await MainThread.InvokeOnMainThreadAsync(
                    async () => await this.NavigateToPopupPageAsync<TResult>(popupPageKey, animated, parameter));
            }

            if (!PopupPageKeyToPageMap.TryGetValue(
                popupPageKey,
                out PopupPageInfo popupPageInfo))
            {
                throw new ArgumentException(
                    $"invalid popup page key: {popupPageKey}",
                    nameof(popupPageKey));
            }

            Type? parameterType = popupPageInfo.ParameterType;

            if (parameterType != null &&
                parameter != null &&
                !(parameterType.Equals(parameter.GetType()) ||
                  parameter.GetType().IsSubclassOf(parameterType)))
            {
                throw new ArgumentException(
                    $"expected parameter of type {parameterType.FullName}, but type {parameter.GetType().FullName} was passed!",
                    nameof(parameter));
            }

            IServiceProvider serviceProvider =
                IPlatformApplication.Current?.Services
                ?? throw new InvalidOperationException("Serice provider is not available");

            Type popupPageType = popupPageInfo.PopupPageType;

            Popup? popupPage = null;
            if (parameter == null)
            {
                popupPage = (Popup?)ActivatorUtilities.CreateInstance(
                    serviceProvider,
                    popupPageType);
            }
            else
            {
                popupPage = (Popup?)Activator.CreateInstance(
                    popupPageType,
                    parameter);
            }

            if (popupPage == null)
            {
                throw new InvalidOperationException("couldn't create popup page for popup page key " + popupPageKey);
            }

            Type? returnType = popupPageInfo.ReturnType;
            if (returnType != null &&
                typeof(TResult) != returnType)
            {
                Debug.Assert(
                    false,
                    $"the page's {popupPage.GetType().FullName} result type {returnType?.FullName} doesn't match the calling NavigateToPopupPageAsync result type {typeof(TResult).FullName}");
            }

            if (popupPage is Popup<TResult> popupPageWithResult)
            {
                IPopupResult<TResult> result =
                    await UserInterface.MainPage.ShowPopupAsync<TResult>(
                        popupPageWithResult,
                        DefaultPopupOptions);

                return result.Result;
            }
            else
            {
                await UserInterface.MainPage.ShowPopupAsync(
                    popupPage,
                    DefaultPopupOptions);

                return null;
            }
        }

        /// <summary>
        /// Navigates back in navigation stack
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task GoBack()
        {
            if (this.NavigationPage == null)
            {
                throw new InvalidOperationException("Navigation page wasn't initialized properly");
            }

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
        public async Task NavigateAsync(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            Type pageType,
            bool animated = true,
            object? parameter = null)
        {
            if (this.NavigationPage == null)
            {
                throw new InvalidOperationException("Navigation page wasn't initialized properly");
            }

            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(async () => await this.NavigateAsync(pageType, animated, parameter));
                return;
            }

            // close flyout if necessary
            if (UserInterface.MainPage is FlyoutPage flyoutPage &&
                flyoutPage.IsPresented)
            {
                flyoutPage.IsPresented = false;
            }

            IServiceProvider serviceProvider =
                IPlatformApplication.Current?.Services
                ?? throw new InvalidOperationException("Serice provider is not available");

            Page? displayPage = null;
            if (pageType == typeof(MapPage))
            {
                await this.NavigationPage.Navigation.PopToRootAsync();
            }
            else if (parameter == null)
            {
                displayPage = (Page?)ActivatorUtilities.CreateInstance(
                    serviceProvider,
                    pageType);
            }
            else
            {
                displayPage = (Page?)ActivatorUtilities.CreateInstance(
                    serviceProvider,
                    pageType,
                    parameter);
            }

            if (displayPage != null)
            {
                await this.NavigationPage.PushAsync(displayPage, animated);
            }
        }
    }
}
