# Readme for UI-Tests

This project contains the UI tests for Where-to-fly. The tests use Appium and
can run on Android or Windows. To test debug versions of the app, deploy the
app on the device. For release tests, create a package of the app (.apk or
.msixbundle) and copy it to the folder containing the test assembly.

In order to install appium, follow these steps:
https://learn.microsoft.com/en-us/samples/dotnet/maui-samples/uitest-appium-nunit/

The tests can be controlled using the following envrionment variables:

- `UITEST_PLATFORM`: Set to Android to test on Android, or to Windows to test
  on Windows.
- `UITEST_APPID`: Set to the Android Package ID or Windows Package ID of the
  app to test.
