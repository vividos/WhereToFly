namespace WhereToFly.Logic
{
    /// <summary>
    /// HTML conversion methods
    /// </summary>
    public static class HtmlConverter
    {
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
