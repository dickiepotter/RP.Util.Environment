using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("RP.Util.Environment")]
[assembly: AssemblyDescription("Utility class for checking the system and security environment an application is running in")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("RP")]
[assembly: AssemblyProduct("Environment")]
[assembly: AssemblyCopyright("Copyright © Richard Potter 2016")]

[assembly: ComVisible(false)]

[assembly: Guid("3bb8fe2f-5c5f-4657-b6bb-b695568c07a9")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
