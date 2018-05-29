using Android.App;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("WhereToFly.App.Android")]
[assembly: AssemblyDescription("Where-to-fly Android app")]
[assembly: AssemblyConfiguration("armeabi-v7a")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("WhereToFly.App.Android")]
[assembly: AssemblyCopyright("Copyright © 2017-2018 Michael Fink")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.3.0.*")]
[assembly: AssemblyFileVersion("1.3.0.0")]

// Add some common permissions; duplicated from AndroidManifest.xml
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessCoarseLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessFineLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]

// set debuggable flag based on configuration
#if DEBUG
[assembly: Application(Debuggable = true)]
#else
[assembly: Application(Debuggable = false)]
#endif
