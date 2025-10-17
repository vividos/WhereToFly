using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Logic;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests for class HtmlConverter
    /// </summary>
    [TestClass]
    public class HtmlConverterTest
    {
        /// <summary>
        /// Tests method FromMarkdown(), with empty text and no font set
        /// </summary>
        [TestMethod]
        public void TestFromMarkdown_EmptyText()
        {
            // run
            string html = HtmlConverter.FromMarkdown(string.Empty, null);

            // check
            Assert.AreEqual(0, html.Length, "html text must also be empty");
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
            Assert.Contains(FontName, html, "html text must contain font name");
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
            Assert.IsGreaterThan(0, html.Length, "html text must not be empty");
            Assert.Contains("Heading", html, "must contain heading");
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
[Internet Link](https://www.cesium.com/)
[![Image Link](https://sonarcloud.io/api/badges/gate?key=WhereToFly)](https://sonarcloud.io/dashboard?id=WhereToFly)
- [ ] CheckBox
";

            // run
            string html = HtmlConverter.FromMarkdown(markdown);

            // check
            Assert.IsGreaterThan(0, html.Length, "html text must not be empty");

            Assert.Contains("Heading", html, "must contain heading");
            Assert.Contains("Sub Heading", html, "must contain sub heading");
            Assert.Contains("LocalLink", html, "must contain local link target");
            Assert.Contains("Local Link", html, "must contain local link text");
            Assert.Contains("github.com/vividos", html, "must contain text-less internet link URL");
            Assert.Contains("Internet Link", html, "must contain internet link text");
            Assert.Contains("www.cesium.com", html, "must contain internet link target");
            Assert.Contains("Image Link", html, "must contain image link text");
            Assert.Contains("CheckBox", html, "must contain checkbox text");
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
            Assert.AreEqual(0, html.Length, "sanitized html text must also be empty");
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
            Assert.AreEqual(-1, html.IndexOf("<script"), "sanitized html text must not contain script tag");
        }
    }
}
