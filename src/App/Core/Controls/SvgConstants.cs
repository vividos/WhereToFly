namespace WhereToFly.App.Core.Controls
{
    /// <summary>
    /// Common constants for SVG images
    /// </summary>
    public static class SvgConstants
    {
        /// <summary>
        /// Resource URI prefix
        /// </summary>
        public const string ResourceUriPrefix = "resource://";

        /// <summary>
        /// Query part of a resource URI to specify the assembly
        /// </summary>
        public const string ResourceUriAssemblyQuery = "?assembly=";

        /// <summary>
        /// Prefix of a data URI that contains plain SVG image text
        /// </summary>
        public const string DataUriPlainPrefix = "data:image/svg+xml,";

        /// <summary>
        /// Prefix of a data URI that contains Base64 encoded SVG image text
        /// </summary>
        public const string DataUriBase64Prefix = "data:image/svg+xml;base64,";
    }
}
