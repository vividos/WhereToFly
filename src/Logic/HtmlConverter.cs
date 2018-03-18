namespace WhereToFly.Logic
{
    /// <summary>
    /// HTML conversion methods
    /// </summary>
    public static class HtmlConverter
    {
        /// <summary>
        /// Sanitizes any potentially dangerous tags from the provided raw HTML input using
        /// a whitelist based approach, leaving the "safe" HTML tags.
        /// </summary>
        /// <param name="htmlText">HTML text to sanitize</param>
        /// <returns>sanitized HTML text</returns>
        public static string Sanitize(string htmlText)
        {
            // TODO implement
            return htmlText;
        }

        /// <summary>
        /// Converts from Markdown text to HTML
        /// </summary>
        /// <param name="markdownText">Markdown formatted text</param>
        /// <returns>html text</returns>
        public static string FromMarkdown(string markdownText)
        {
            string htmlText = CommonMark.CommonMarkConverter.Convert(markdownText);
            return htmlText;
        }
    }
}
