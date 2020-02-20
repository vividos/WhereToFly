using Android.App;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("WhereToFly.App.Android")]
[assembly: AssemblyDescription("Where-to-fly Android app")]
[assembly: AssemblyConfiguration("armeabi-v7a")]

[assembly: ComVisible(false)]

// Add some common permissions; duplicated from AndroidManifest.xml
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessCoarseLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessFineLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessNetworkState)]

// set debuggable flag based on configuration
#if DEBUG
[assembly: Application(Debuggable = true)]
#else
[assembly: Application(Debuggable = false)]
#endif
