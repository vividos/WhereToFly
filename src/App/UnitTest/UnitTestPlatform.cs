﻿using System.IO;
using WhereToFly.App.Core;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// IPlatform implementation for unit tests
    /// </summary>
    internal class UnitTestPlatform : IPlatform
    {
        /// <summary>
        /// Returns web view base path; always "about:blank"
        /// </summary>
        public string WebViewBasePath => "about:blank";

        /// <summary>
        /// Loads text document from assets
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>asset text</returns>
        public string LoadAssetText(string assetFilename)
        {
            using (var stream = this.OpenAssetStream(assetFilename))
            {
                var streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Opens asset stream; not implemented, throws exception
        /// </summary>
        /// <param name="assetFilename">asset filename</param>
        /// <returns>nothing, always throws exception</returns>
        public Stream OpenAssetStream(string assetFilename)
        {
            string filename = Path.Combine(
                Path.GetDirectoryName(this.GetType().Assembly.Location),
                "Assets",
                assetFilename);

            return new FileStream(filename, FileMode.Open);
        }

        /// <summary>
        /// Sets app theme to use for platform
        /// </summary>
        /// <param name="requestedTheme">requested theme</param>
        public void SetPlatformTheme(OSAppTheme requestedTheme)
        {
            // nothing to do
        }
    }
}
