using HtmlAgilityPack;
using System.Collections.Generic;

namespace WhereToFly.Logic
{
    /// <summary>
    /// HTML conversion methods
    /// </summary>
    public static class HtmlConverter
    {
        /// <summary>
        /// Sanitizes any potentially dangerous tags from the provided raw HTML input using
        /// a whitelist based approach, leaving the "safe" HTML tags. See:
        /// https://stackoverflow.com/questions/12787449/html-agility-pack-removing-unwanted-tags-without-removing-content
        /// </summary>
        /// <param name="htmlText">HTML text to sanitize</param>
        /// <returns>sanitized HTML text</returns>
        public static string Sanitize(string htmlText)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlText);

            var acceptableTags = new List<string>
            {
                "div", "span",
                "p", "b", "i", "strong", "em",
                "table", "th", "tr", "td",
                "ul", "li",
                "img", "a"
            };

            ReplaceUnwantedTags(htmlDocument.DocumentNode, acceptableTags);

            return htmlDocument.DocumentNode.InnerHtml;
        }

        /// <summary>
        /// Replaces all unwanted tags by examining all nodes and keep acceptable tags
        /// </summary>
        /// <param name="rootNode">root node to inspect</param>
        /// <param name="acceptableTags">list of acceptable tags</param>
        private static void ReplaceUnwantedTags(HtmlNode rootNode, List<string> acceptableTags)
        {
            var nodes = new Queue<HtmlNode>(rootNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (!acceptableTags.Contains(node.Name) && node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);
                }
            }
        }

        /// <summary>
        /// Strips all HTML tags, leaving only the plain text, e.g. to display in non-HTML UI
        /// elements.
        /// </summary>
        /// <param name="htmlText">HTML text to strip</param>
        /// <returns>non-HTML text</returns>
        public static string StripAllTags(string htmlText)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlText);

            string result = htmlDocument.DocumentNode.InnerText;
            return result.Replace("  ", " ").Replace("\r", string.Empty).Replace("\n", string.Empty);
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
