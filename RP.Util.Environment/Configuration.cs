using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

using RP.Util;

namespace RP.Util
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;

    using Microsoft.Web.XmlTransform;

    /// <summary>
    /// A configuration file class that lets us apply application level configuration file transforms stored in various locations on the local machine when in development mode.
    /// Global configuration file transforms are stored in a path specified by a DevTransformPath system evnvironment variable.
    /// Local configuration file transforms are stored in the same folder as the executable or in the same folder as (or a config folder under) the .csproj file when using the standard \bin\debug build path.
    /// The name of the config file should look like [application].[user].[machine].config, [application].[user].config, or, [application].[machine].config. There are other variations but this should be aimed for.
    /// </summary>
    public static class Configuration
    {
        #region Dev Transform

        #region Transform constants and variables

        private const string EnvironmentVariableNameForGlobalConfigFileTranformsPath = "DevTransformPath";

        private static readonly string[] ConfigTransformFileExtensions = new[] {".config", ".configtransform", ".transform"};

        #endregion

        #region Transform Files

        public static string GlobalDevConfigTransformFolder
        {
            get
            {
                return System.Environment.GetEnvironmentVariable(
                    EnvironmentVariableNameForGlobalConfigFileTranformsPath);
            }
        }

        public static IEnumerable<string> LocalDevConfigTransformFolders
        {
            get
            {
                if (!SecurityEnvironment.IsInDevelopment)
                {
                    return new string[] { };
                }

                var paths = new List<string>();

                var nextToExe = Path.Combine(RuntimeEnvironment.MainAssemblyFolder);                                                                                              // Next to executable
                var nextToSourceProject = Path.Combine( Regex.Replace(RuntimeEnvironment.MainAssemblyFolder, @"\\bin\\debug", string.Empty, RegexOptions.IgnoreCase));            // Next to .csproj if using the default build path 
                var inFolderNextToSourceProject =  Path.Combine(Regex.Replace(RuntimeEnvironment.MainAssemblyFolder, @"\\bin\\debug", @"\Config", RegexOptions.IgnoreCase));      // In a config folder uneder the folder holding the .csproj if using the default build path

                paths.Add(nextToExe);
                paths.Add(nextToSourceProject);
                paths.Add(inFolderNextToSourceProject);

                return paths;
            }
        }

        public static IEnumerable<string> GetDevConfigTransformFiles()
        {
            if (!SecurityEnvironment.IsInDevelopment)
            {
                return new string[] { };
            }

            IEnumerable<string> globalTransformFiles = new string[] { };
            IEnumerable<string> localTransformFiles = new string[] { };

            if (IsValidDevConfigTransformFolder(GlobalDevConfigTransformFolder))
            {
                globalTransformFiles =
                    Directory.GetFiles(GlobalDevConfigTransformFolder).Where(file => IsValidDevConfigTransformFile(file, true));
            }

            LocalDevConfigTransformFolders
                .Where(IsValidDevConfigTransformFolder)
                .SelectMany(folder => Directory.GetFiles(folder), (folder, file) => file)
                .Where(file => IsValidDevConfigTransformFile(file, false));

            return globalTransformFiles.Concat(localTransformFiles);
        }

        private static bool IsValidDevConfigTransformFolder(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }

        private static bool IsValidDevConfigTransformFile(string path, bool isFromGlobalDevConfigTransformFolder = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            // Check the file extension
            var extension = Path.GetExtension(path).ToLower();
            bool hasValidExtension = ConfigTransformFileExtensions.Any(x => extension.Equals(extension));

            // Setup the environment variables to check for
            var machineName = RuntimeEnvironment.MachineName.ToLower().Replace('.', '_');
            var userName = SecurityEnvironment.WindowsUsernameWithoutDomain.ToLower().Replace('.', '_');
            var mainAssemblyName = RuntimeEnvironment.MainAssemblyName.ToLower().Replace('.', '_');
            var mainAssemblyFileName = RuntimeEnvironment.MainAssemblyFileName.ToLower().Replace('.', '_');
           
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path).ToLower().Replace('.', '_');

            // Check the file name (Global Folder)
            if (isFromGlobalDevConfigTransformFolder)
            {
                return
                    hasValidExtension && 
                    (
                        fileNameWithoutExtension.Equals(mainAssemblyName) ||
                        fileNameWithoutExtension.Equals(string.Format("{0}_{1}", mainAssemblyName, machineName)) ||
                        fileNameWithoutExtension.Equals(string.Format("{0}_{1}", mainAssemblyFileName, machineName)) ||
                        fileNameWithoutExtension.Equals(string.Format("{0}_{1}", mainAssemblyName, userName)) ||
                        fileNameWithoutExtension.Equals(string.Format("{0}_{1}", mainAssemblyFileName, userName)) ||
                        (
                            fileNameWithoutExtension.Contains(mainAssemblyName) &&
                            fileNameWithoutExtension.Contains(machineName) &&
                            fileNameWithoutExtension.Contains(userName)
                        )
                     );
            }

            // Check the file name (Local Folder)
            return
               hasValidExtension &&
                (
                    fileNameWithoutExtension.Equals(machineName) ||
                    fileNameWithoutExtension.Equals(userName) ||
                    fileNameWithoutExtension.Equals(string.Format("app_{0}", machineName)) ||
                    fileNameWithoutExtension.Equals(string.Format("web_{0}", userName)) ||
                    fileNameWithoutExtension.Equals(string.Format("{0}_{1}", mainAssemblyName, machineName)) ||
                    fileNameWithoutExtension.Equals(string.Format("{0}_{1}", mainAssemblyName, userName)) ||
                    fileNameWithoutExtension.Equals(string.Format("{0}_{1}", mainAssemblyFileName, machineName)) ||
                    fileNameWithoutExtension.Equals(string.Format("{0}_{1}", mainAssemblyFileName, userName)) ||
                    (
                        fileNameWithoutExtension.Contains(machineName) &&
                        fileNameWithoutExtension.Contains(userName)
                    )
                );
        }

        #endregion

        #region ApplyTransforms

        private static System.Configuration.Configuration GetTransformedApplicationConfiguration()
        {
            // todo: does this work for web projects
            var appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appConfigPath = appConfig.FilePath;

            var xmlAppConfig = new XmlTransformableDocument();
            xmlAppConfig.Load(appConfigPath);

            foreach (var devConfigTransformFile in GetDevConfigTransformFiles())
            {
                ApplyTransform(xmlAppConfig, devConfigTransformFile);
            }

            var transformedConfigPath = appConfigPath + ".DevTransform";
            xmlAppConfig.Save(transformedConfigPath);

            var transformedConfig = 
                ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap() { ExeConfigFilename = transformedConfigPath },
                    ConfigurationUserLevel.None);

            return transformedConfig;
        }

        private static void ApplyTransform(XmlTransformableDocument config, string transformFile)
        {
            // Logging
            IXmlTransformationLogger logger = null; // ToDo Create a concrete class for and instanciate an IXmlTransformationLogger and pass into XmlTransformation constructor

            // Create a transformation object
            using (var transform = new XmlTransformation(transformFile))
            {
                // Transform
                transform.Apply(config);
            }
        }

        #endregion

        #endregion

        #region Configuration File

        private static System.Configuration.Configuration internalConfiguration = null;

        private static System.Configuration.Configuration InternalConfiguration
        {
            get
            {
                return internalConfiguration ?? (internalConfiguration = GetTransformedApplicationConfiguration());
            }
        }

        #endregion

        #region Config Value Extraction

        public static string GetConnectionString(string key, string defaultConnectionString = null)
        {
            return InternalConfiguration.ConnectionStrings.ConnectionStrings["key"].ConnectionString ?? defaultConnectionString;
        }

        public static string GetAppSetting(string key, string defaultValue = null)
        {
            return InternalConfiguration.AppSettings.Settings.AllKeys.Contains(key)
                       ? ConfigurationManager.AppSettings["key"]
                       : defaultValue;
        }

        #endregion
    }
}
