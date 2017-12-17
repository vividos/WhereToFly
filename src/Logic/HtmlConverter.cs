using System.Diagnostics;
using System.Globalization;
using WhereToFly.Logic.Model;

namespace WhereToFly.Logic
{
    public static class HtmlConverter
    {
        public static string FromMarkdown(string markdownText)
        {
            string htmlText = CommonMark.CommonMarkConverter.Convert(markdownText);
            return htmlText;
        }
    }
}
