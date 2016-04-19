namespace RP.Util
{
    using System.IO;
    using System.Reflection;

    public static class RuntimeEnvironment
    {
        /// <summary>
        /// Get the path to the executable or first loaded assembly
        /// </summary>
        public static string MainAssemblyPath
        {
            get
            {
                // TODD: This will not work in a web app
                return Path.GetFullPath(Assembly.GetEntryAssembly().Location);
            }
        }

        /// <summary>
        /// Get the path to the folder/directory containing the executable or first loaded assembly
        /// </summary>
        public static string MainAssemblyFolder
        {
            get
            {
                return Path.GetDirectoryName(MainAssemblyPath);
            }
        }

        /// <summary>
        /// Get the name of the the executable or first loaded assembly (with the file extension)
        /// </summary>
        public static string MainAssemblyFileName
        {
            get
            {
                return Path.GetFileName(MainAssemblyPath);
            }
        }

        /// <summary>
        /// Get the name of the the executable or first loaded assembly (without the file extension)
        /// </summary>
        public static string MainAssemblyName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(MainAssemblyPath);
            }
        }

        /// <summary>
        /// Gets the NetBIOS name of this Local Machine
        /// </summary>
        public static string MachineName
        {
            get
            {
                return System.Environment.MachineName;
            }
        }
    }
}