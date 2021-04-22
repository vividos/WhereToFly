using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.Logic;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for class HtmlConverter
    /// </summary>
    [TestClass]
    public class HtmlConverterTest
    {
        /// <summary>
        /// Tests method FromMarkdown(), with empty text
        /// </summary>
        [TestMethod]
        public void TestFromMarkdown_EmptyText()
        {
            // run
            string html = HtmlConverter.FromMarkdown(string.Empty, fontName: null);

            // check
            Assert.IsTrue(html.Length == 0, "html text must also be empty");
        }

        /// <summary>
        /// Tests method FromMarkdown(), with empty text but a font set
        /// </summary>
        [TestMethod]
        public void TestFromMarkdown_EmptyTextButFont()
        {
            const string FontName = "FunnyFont";

            // run
            string html = HtmlConverter.FromMarkdown(string.Empty, fontName: FontName);

            // check
            Assert.IsTrue(html.Contains(FontName), "html text must contain font name");
        }

        /// <summary>
        /// Tests method FromMarkdown(), with heading text
        /// </summary>
        [TestMethod]
        public void TestFromMarkdown_HeadingText()
        {
            // run
            string html = HtmlConverter.FromMarkdown("# Heading");

            // check
            Assert.IsTrue(html.Length > 0, "html text must not be empty");
            Assert.IsTrue(html.Contains("Heading"), "must contain heading");
        }

        /// <summary>
        /// Tests method FromMarkdown(), with heading text
        /// </summary>
        [TestMethod]
        public void TestFromMarkdown_MarkdownText()
        {
            // set up
            string markdown = @"# Heading
## Sub Heading
[LocalLink.md](LocalLink.md ""Local Link"")
https://github.com/vividos/WhereToFly
[Internet Link](https://cesiumjs.org/)
[![Image Link](https://sonarcloud.io/api/badges/gate?key=WhereToFly)](https://sonarcloud.io/dashboard?id=WhereToFly)
- [ ] CheckBox
";

            // run
            string html = HtmlConverter.FromMarkdown(markdown);

            // check
            Assert.IsTrue(html.Length > 0, "html text must not be empty");

            Assert.IsTrue(html.Contains("Heading"), "must contain heading");
            Assert.IsTrue(html.Contains("Sub Heading"), "must contain sub heading");
            Assert.IsTrue(html.Contains("LocalLink"), "must contain local link target");
            Assert.IsTrue(html.Contains("Local Link"), "must contain local link text");
            Assert.IsTrue(html.Contains("github.com/vividos"), "must contain text-less internet link URL");
            Assert.IsTrue(html.Contains("Internet Link"), "must contain internet link text");
            Assert.IsTrue(html.Contains("cesiumjs.org"), "must contain internet link target");
            Assert.IsTrue(html.Contains("Image Link"), "must contain image link text");
            Assert.IsTrue(html.Contains("CheckBox"), "must contain checkbox text");
        }

        /// <summary>
        /// Tests method Sanitize(), with empty text
        /// </summary>
        [TestMethod]
        public void TestSanitize_EmptyText()
        {
            // run
            string html = HtmlConverter.Sanitize(string.Empty);

            // check
            Assert.IsTrue(html.Length == 0, "sanitized html text must also be empty");
        }

        /// <summary>
        /// Tests method Sanitize(), with HTML containing unwanted tag
        /// </summary>
        [TestMethod]
        public void TestSanitize_UnwantedTag()
        {
            // run
            string text = "hello <script type=\"javascript\">alert(\"hello world\");</script>world";
            string html = HtmlConverter.Sanitize(text);

            // check
            Assert.IsTrue(html.IndexOf("<script") == -1, "sanitized html text must not contain script tag");
        }
    }
}
