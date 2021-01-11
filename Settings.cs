using System.Diagnostics;
using System.Reflection;

namespace DuckScript
{
    class Settings
    {
        #region Settings
        public static string username = "";
        public static string webhook = "";
        public static string errorwebhook = "";
        public static string errors = "true";
        public static string debug = "true";
        public static string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        #endregion
    }
}
    