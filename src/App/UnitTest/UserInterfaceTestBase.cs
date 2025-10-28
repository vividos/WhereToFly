using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.Abstractions;
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
        /// <summary>
        /// Services for unit tests
        /// </summary>
        public IServiceProvider Services { get; private set; }
            = new ServiceCollection().BuildServiceProvider();

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

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<CompassGeoServices>();
            serviceCollection.AddSingleton<IAppMapService, UnitTestAppMapService>();
            serviceCollection.AddSingleton<IUserInterface, UnitTestUserInterface>();
            serviceCollection.AddSingleton<IGeolocationService, UnitTestGeolocationService>();
            serviceCollection.AddSingleton<IDataService, SqliteDatabaseDataService>();
            serviceCollection.AddSingleton<INavigationService, UnitTestNavigationService>();
            serviceCollection.AddSingleton<IAppManager, UnitTestAppManager>();

            this.Services = serviceCollection.BuildServiceProvider();

            DependencyService.RegisterSingleton(this.Services.GetRequiredService<CompassGeoServices>());
            DependencyService.RegisterSingleton(this.Services.GetRequiredService<IAppMapService>());
            DependencyService.RegisterSingleton(this.Services.GetRequiredService<IUserInterface>());
            DependencyService.RegisterSingleton(this.Services.GetRequiredService<IGeolocationService>());
            DependencyService.RegisterSingleton(this.Services.GetRequiredService<IDataService>());
            DependencyService.RegisterSingleton(this.Services.GetRequiredService<INavigationService>());
            DependencyService.RegisterSingleton(this.Services.GetRequiredService<IAppManager>());

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

            Microsoft.Maui.Controls.Xaml.ResourceDictionaryHelpers.LoadFromSource(
                resourceDictionary,
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
