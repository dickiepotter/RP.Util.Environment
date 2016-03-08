namespace RP.Util.Environment.TestConsole
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IsInDevelopment: " + SecurityEnvironment.IsInDevelopment());
            Console.WriteLine("IsSafeToBypassLogin: " + SecurityEnvironment.IsSafeToBypassLogin());
            Console.WriteLine("IsLocalAdministrator: " + SecurityEnvironment.IsLocalAdministrator());
            Console.ReadKey();
        }
    }
}
