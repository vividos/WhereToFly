using UIKit;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.StyleCop.CSharp.NamingRules",
    "SA1300:ElementMustBeginWithUpperCaseLetter",
    Scope = "namespace",
    Target = "~N:WhereToFly.App.iOS",
    Justification = "iOS is a proper name")]

namespace WhereToFly.App.iOS
{
    /// <summary>
    /// Application object for ths iOS app
    /// </summary>
    public static class Application
    {
        /// <summary>
        /// This is the main entry point of the application.
        /// </summary>
        /// <param name="args">arguments; unused</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Sonar Code Smell",
            "S4823:Using command line arguments is security-sensitive",
            Justification = "Xamarin.iOS boilerplate code")]
        public static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}
