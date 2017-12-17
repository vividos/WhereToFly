using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.Core.Views;
using Xamarin.Forms;

namespace WhereToFly.Core.Services
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
                case Constants.PageKeyLocationDetailsPage:
                    pageType = typeof(LocationDetailsPage);
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

            if (pageType != null)
            {
                await this.NavigateAsync(pageType, animated, parameter);
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

            Page displayPage;
            if (parameter == null)
            {
                displayPage = (Page)Activator.CreateInstance(pageType);
            }
            else
            {
                displayPage = (Page)Activator.CreateInstance(pageType, parameter);
            }

            await this.NavigationPage.PushAsync(displayPage, animated: true);
        }
    }
}
