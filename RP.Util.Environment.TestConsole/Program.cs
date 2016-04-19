namespace RP.Util.Environment.TestConsole
{
    using System;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Security Environment::");
            Console.WriteLine("IsInDevelopment: " + SecurityEnvironment.IsInDevelopment);
            Console.WriteLine("IsSafeToBypassLogin: " + SecurityEnvironment.IsSafeToBypassLogin);
            Console.WriteLine("IsLocalAdministrator: " + SecurityEnvironment.IsLocalAdministrator);
            Console.WriteLine("UserName: " + SecurityEnvironment.WindowsUsername);
            Console.WriteLine("UserNameWithoutDomain: " + SecurityEnvironment.WindowsUsernameWithoutDomain);
            Console.WriteLine();

            Console.WriteLine("Runtime Environment::");
            Console.WriteLine("MachineName: " + RuntimeEnvironment.MachineName);
            Console.WriteLine("MainAssemblyFileName: " + RuntimeEnvironment.MainAssemblyFileName);
            Console.WriteLine("MainAssemblyName: " + RuntimeEnvironment.MainAssemblyName);
            Console.WriteLine("MainAssemblyFolder: " + RuntimeEnvironment.MainAssemblyFolder);
            Console.WriteLine("MainAssemblyPath: " + RuntimeEnvironment.MainAssemblyPath);
            Console.WriteLine();

            Console.WriteLine("Configuration::");
            Console.WriteLine("GlobalDevConfigTransformFolder: " + Configuration.GlobalDevConfigTransformFolder);
            Console.WriteLine("LocalDevConfigTransformFolders:");
            foreach (var possibleLocalAssemblyPath in Configuration.LocalDevConfigTransformFolders)
            {
                Console.WriteLine("   * " + possibleLocalAssemblyPath);
            }
            Console.WriteLine("GetDevConfigTransformFiles:");
            foreach (var possibleConfigTransformPath in Configuration.GetDevConfigTransformFiles())
            {
                Console.WriteLine("   * " + possibleConfigTransformPath);
            }
            //Console.WriteLine("Config Transform:");
            //var stream = ((FileStream)Configuration.GetConfigTransform());
            //Console.WriteLine("   Bytes: " + (stream == null ? "null" : stream.Length.ToString()));
            //Console.WriteLine("   Name: " + (stream == null ? "null" : stream.Length.ToString()));

            Console.ReadKey();
        }
    }
}
