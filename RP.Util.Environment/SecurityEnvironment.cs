namespace RP.Util
{
    using System;
    using System.Configuration;
    using System.Deployment.Application;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Principal;

    /// <summary>
    /// A static class for checking the level of security needed, given an application's operating environment and configuration
    /// </summary>
    public static class SecurityEnvironment
    {
        /// <summary>
        /// Check if the application is in Development mode (This requires an AppSetting Environment of Develop, Debug compile and cannot have been deployed using clickOnce)
        /// </summary>
        /// <returns>True if the application is in development mode</returns>
        public static bool IsInDevelopment
        {
            get
            {
                // Does the configuration specify an AppSetting called Environment with the value Development
                bool validAppSetting;

                try
                {
                    validAppSetting = (ConfigurationManager.AppSettings.AllKeys.Contains("Environment")
                                       && ConfigurationManager.AppSettings["Environment"].Equals(
                                           "development",
                                           StringComparison.OrdinalIgnoreCase))
                                      || (ConfigurationManager.AppSettings.AllKeys.Contains("environment")
                                          && ConfigurationManager.AppSettings["environment"].Equals(
                                              "development",
                                              StringComparison.OrdinalIgnoreCase));
                }
                catch (ConfigurationErrorsException)
                {
                    // Could not retrive the configuration setting, so it must be missing
                    validAppSetting = false;
                }

                // Has the application been built with debugging enabled
                var validBuildFlags =
                    Assembly.GetEntryAssembly()
                        .GetCustomAttributes(false)
                        .OfType<DebuggableAttribute>()
                        .Any(x => x.IsJITTrackingEnabled);

                // Is the application deployed using ClickOnce
                var validNetworkDeploy = !ApplicationDeployment.IsNetworkDeployed;

                return validAppSetting && validBuildFlags && validNetworkDeploy;
            }
        }

        /// <summary>
        /// Check if it is reasonably safe to allow logins to be bypassed (This requires the current user to be a local administrator and the application to be in development mode)
        /// </summary>
        /// <returns>True if it is reasonably safe to allow login to be bypassed</returns>
        public static bool IsSafeToBypassLogin
        {
            get
            {
                return IsInDevelopment && IsLocalAdministrator;
            }
        }

        /// <summary>
        /// Is the current Windows user a local machine administrator
        /// </summary>
        /// <returns>True if the current user is a local machine administrator</returns>
        public static bool IsLocalAdministrator
        {
            get
            {
                try
                {
                    var identity = WindowsIdentity.GetCurrent();

                    if (identity == null)
                    {
                        return false;
                    }

                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (SecurityException)
                {
                    // Did not have permission to check permissions, so obviously not an admin
                    return false;
                }
            }
        }

        public static string WindowsUsername
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();

                if (identity == null)
                {
                    return null;
                }

                return identity.Name;
            }
        }

        public static string WindowsUsernameWithoutDomain
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();

                if (identity == null)
                {
                    return null;
                }

                var name = identity.Name;
                return name.Substring(name.IndexOf('\\')).TrimStart('\\');
            }
        }
    }
}
