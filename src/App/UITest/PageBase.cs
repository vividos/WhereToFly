using Xamarin.UITest;

namespace WhereToFly.App.UITest
{
    /// <summary>
    /// UI test page base class; using the UI test Page Object Pattern
    /// </summary>
    internal class PageBase
    {
        /// <summary>
        /// App under test
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        protected readonly IApp app;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Creates a new page base class object; call this from the page classes
        /// </summary>
        /// <param name="app">app under test</param>
        protected PageBase(IApp app)
        {
            this.app = app;
        }

        /// <summary>
        /// Opens page and returns new page object
        /// </summary>
        /// <param name="page">page to open</param>
        /// <returns>new page object</returns>
        public PageBase OpenPage(PageToOpen page)
        {
            this.app.Tap(x => x.Class("AppCompatImageButton"));

            // the PageToOpen enum has the same values as the AutomationId
            // attributes on the menu plus a suffix
            this.app.Tap(x => x.Marked($"{page}Page"));

            return page switch
            {
                PageToOpen.Map => new MapPage(this.app),
                _ => new PageBase(this.app),
            };
        }
    }
}
