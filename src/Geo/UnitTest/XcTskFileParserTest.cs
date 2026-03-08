using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WhereToFly.Geo.DataFormats;

namespace WhereToFly.Geo.UnitTest;

/// <summary>
/// Tests for <see cref="XcTskFileParser"/> class
/// </summary>
[TestClass]
public class XcTskFileParserTest
{
    /// <summary>
    /// Tests loading version 1 file
    /// </summary>
    [TestMethod]
    public void TestParseVersion1File()
    {
        // set up
        string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "task_version1.xctsk");

        using var stream = new FileStream(filename, FileMode.Open);

        // run
        var xctskFileParser = new XcTskFileParser(filename, stream);

        string czml = xctskFileParser.ConvertToCzml();

        // check
        Assert.IsNotEmpty(xctskFileParser.Description, "description must be non-empty");
        Assert.IsNotEmpty(czml, "CZML must be non-empty");
    }
}
