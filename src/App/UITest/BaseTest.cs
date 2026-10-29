using OpenQA.Selenium.Appium.Enums;
using Plugin.Maui.UITestHelpers.Core;
using System.Diagnostics;
using UITest.Appium.NUnit;

namespace WhereToFly.App.UITest;

/// <summary>
/// UI test fixture base class
/// </summary>
public abstract class BaseTest : UITestBase
{
    /// <summary>
    /// Creates a new base test object
    /// </summary>
    protected BaseTest()
        : base(GetTestPlatform())
    {
    }

    /// <summary>
    /// Gets the test platform, based on the UITEST_PLATFORM environment
    /// variable
    /// </summary>
    /// <returns>test device type</returns>
    private static TestDevice GetTestPlatform()
    {
        string? testPlatform =
            Environment.GetEnvironmentVariable("UITEST_PLATFORM");

        Debug.WriteLine("Using Test Platform: " + testPlatform);

        return testPlatform?.ToLowerInvariant() switch
        {
            "android" => TestDevice.Android,
            _ => TestDevice.Windows,
        };
    }

    /// <summary>
    /// Gets test config for UI tests
    /// </summary>
    /// <returns>config object</returns>
    /// <exception cref="InvalidOperationException">
    /// thrown when test config could not be provided
    /// </exception>
    public override IConfig GetTestConfig()
    {
        var config = new Config();

        string? appIdentifier =
            Environment.GetEnvironmentVariable("UITEST_APPID");

        if (!string.IsNullOrEmpty(appIdentifier))
        {
            config.SetProperty("AppId", appIdentifier);
            Debug.WriteLine("Using App ID: " + appIdentifier);
        }

        switch (this._testDevice)
        {
            case TestDevice.Android:
#if DEBUG
                // DEBUG BUILD SETUP
                // If you're running your tests against debug builds you'll need to set NoReset to true
                // otherwise appium will delete all the libraries used for Fast Deployment on Android
                // Release builds have Fast Deployment disabled
                // https://learn.microsoft.com/xamarin/android/deploy-test/building-apps/build-process#fast-deployment
                config.SetProperty(
                    MobileCapabilityType.NoReset,
                    "true");

                config.SetProperty(
                    AndroidMobileCapabilityType.AppPackage,
                    appIdentifier);

                // Make sure to set [Register("com.companyname.applicationname.MainActivity")]
                // on the MainActivity of your android application
                config.SetProperty(
                    AndroidMobileCapabilityType.AppActivity,
                    $"{appIdentifier}.MainActivity");

#else
                // RELEASE BUILD SETUP
                // The full path to the .apk file
                string apkFilename = Path.Join(
                    TestContext.CurrentContext.TestDirectory,
                    $"{appIdentifier}-Signed.apk");

                config.SetProperty("AppPath", apkFilename);
#endif
                break;

            case TestDevice.Windows:
                // not implemented yet
                break;

            default:
                throw new InvalidOperationException(
                    $"invalid test device {this._testDevice}");
        }

        return config;
    }
}
