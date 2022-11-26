namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// This class is necessary when using records in C#, when having a .NET runtime before
    /// .NET 5, which UWP and Xamarin are. See also the following issue:
    /// https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
    /// </summary>
    internal static class IsExternalInit
    {
    }
}
