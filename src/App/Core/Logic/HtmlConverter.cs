using HtmlAgilityPack;
using System.Collections.Generic;

namespace WhereToFly.App.Core.Logic
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
                "div", "span", "p", "br",
                "b", "i", "strong", "em",
                "table", "th", "tr", "td", "thead",
                "ul", "li",
                "img", "a",
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
            var selectedNodes = rootNode.SelectNodes("./*|./text()");

            if (selectedNodes == null)
            {
                return;
            }

            var nodes = new Queue<HtmlNode>(selectedNodes);
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
        /// <param name="fontName">font name for whole text; may be null</param>
        /// <param name="fontSize">font size; used when font name is specified</param>
        /// <returns>HTML text</returns>
        public static string FromMarkdown(string markdownText, string fontName = "sans-serif", int fontSize = 14)
        {
            string htmlText = Markdig.Markdown.ToHtml(markdownText);

            if (string.IsNullOrEmpty(fontName))
            {
                return htmlText;
            }

            return $"<span style=\"font-family: {fontName}; font-size: {fontSize}\">{htmlText}</span>";
        }

        /// <summary>
        /// Formats text that may contain markdown or HTML
        /// </summary>
        /// <param name="markdownOrHtmlText">Markdown or HTML formatted text</param>
        /// <param name="fontName">font name for whole text; may be null</param>
        /// <param name="fontSize">font size; used when font name is specified</param>
        /// <returns>HTML text</returns>
        public static string FromHtmlOrMarkdown(string markdownOrHtmlText, string fontName = "sans-serif", int fontSize = 14)
        {
            if (string.IsNullOrEmpty(markdownOrHtmlText))
            {
                return string.Empty;
            }

            bool hasHtmlTags = markdownOrHtmlText.Contains("<") &&
                (markdownOrHtmlText.Contains("</") || markdownOrHtmlText.Contains("/>"));

            if (!hasHtmlTags)
            {
                // try to format as markdown
                markdownOrHtmlText = FromMarkdown(markdownOrHtmlText, fontName, fontSize);
            }

            return markdownOrHtmlText;
        }

        /// <summary>
        /// Adds CSS styles to html text to set color for text, background and links.
        /// </summary>
        /// <param name="text">text to modify</param>
        /// <param name="textColor">text color, in #RRGGBB format</param>
        /// <param name="backgroundColor">background color</param>
        /// <param name="accentColor">accent color</param>
        /// <returns>modified text</returns>
        public static string AddTextColorStyles(string text, string textColor, string backgroundColor, string accentColor)
        {
            string styles = $"<style> body {{ color: {textColor}; background-color: {backgroundColor}; }} " +
                $"a {{ color: {accentColor}; }} </style>";

            return styles + text;
        }
    }
}
