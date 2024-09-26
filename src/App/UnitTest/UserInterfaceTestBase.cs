using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.Logic;
using WhereToFly.App.Models;
using WhereToFly.App.Services;
using WhereToFly.App.Services.SqliteDatabase;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Base class for user interface based unit tests
    /// </summary>
    public class UserInterfaceTestBase
    {
#pragma warning disable CS0612 // Type or member is obsolete
        /// <summary>
        /// Mock implementation of IFontNamedSizeService
        /// </summary>
        internal class MockFontNamedSizeService : IFontNamedSizeService
        {
            /// <inheritdoc />
            public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
            {
                return 42.0;
            }
        }
#pragma warning restore CS0612 // Type or member is obsolete

        /// <summary>
        /// Sets up unit test for testing user interface
        /// </summary>
        [TestInitialize]
        public void SetUpUnitTestUserInterface()
        {
            MauiMocks.Init();

#pragma warning disable CS0612 // Type or member is obsolete
            DependencyService.Register<IFontNamedSizeService, MockFontNamedSizeService>();
#pragma warning restore CS0612 // Type or member is obsolete

            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<IAppMapService, UnitTestAppMapService>();
            DependencyService.Register<IUserInterface, UnitTestUserInterface>();
            DependencyService.Register<IGeolocationService, UnitTestGeolocationService>();
            DependencyService.Register<IDataService, SqliteDatabaseDataService>();
            DependencyService.Register<INavigationService, UnitTestNavigationService>();
            DependencyService.Register<SvgImageCache>();

            App.Settings = new AppSettings();

            LoadAppResources("Resources/Styles/Colors.xaml");
            LoadAppResources("Resources/Styles/Styles.xaml");
        }

        /// <summary>
        /// Loads resource dictionary from the app assembly and adds it to the unit test app
        /// merged resources
        /// </summary>
        /// <param name="resourcePath">
        /// relative resource path with forward slashes, e.g. Resources/Styles/Colors.xaml
        /// </param>
        private static void LoadAppResources(string resourcePath)
        {
            string dotResourcePath = resourcePath.Replace("/", ".");

            var assembly = typeof(MauiProgram).Assembly;

            string uriWithAssembly =
                $"WhereToFly.App.{dotResourcePath};assembly={assembly.GetName().Name}";

            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.SetAndLoadSource(
                new Uri(uriWithAssembly, UriKind.Relative),
                resourcePath,
                assembly,
                null);

            Application app = App.Current
                ?? throw new InvalidOperationException("App.Current is not available!");

            app.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        /// <summary>
        /// Cleans up unit test for testing user interface
        /// </summary>
        [TestCleanup]
        public void TearDownUnitTestUserInterface()
        {
            MauiMocks.Reset();
        }
    }
}
