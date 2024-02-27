using System.IO;

namespace WhereToFly.Geo.UnitTest
{
    /// <summary>
    /// Helper methods for all Geo unit tests
    /// </summary>
    public static class UnitTestHelper
    {
        /// <summary>
        /// Returns the Assets path for all unit tests; place your test files in the Assets folder
        /// and mark them with "Content" and "Copy if newer".
        /// </summary>
        public static string TestAssetsPath =>
            Path.Combine(
                Path.GetDirectoryName(typeof(UnitTestHelper).Assembly.Location)!,
                "Assets");
    }
}
