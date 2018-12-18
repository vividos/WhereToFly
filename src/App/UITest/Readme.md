# Readme for UI-Tests

In order to run User Interface tests for Where-to-fly, you have to do some
steps:

1. Install the "NUnit 2 Test Adapter" extension. You'll need this as Visual
   Studio won't find your test when the NuGet package "NUnitTestAdapter" is
   not restored in your project's bin folder.

2. Be sure your ANDROID_HOME environment variable is set; otherwise starting
   up the tests don't work. Mine is set to:
   ANDROID_HOME = C:\Program Files (x86)\Android\android-sdk

   That's the same path that can be found in:
   Tools > Options > Xamarin > Android SDK Location

3. Build the Release version of the WhereToFly.App.Android project and deploy
   it to the Android device.

4. Connect a device - only one should be connected, or UITest doesn't know
   which one to use.

5. Start the UI tests using the "Test Explorer" window.
