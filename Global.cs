#region using
using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
#endregion

namespace DuckScript
{
    class Global
    {
        #region Functions
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static Random rnd = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[rnd.Next(s.Length)]).ToArray());
        }

        public static string FixString(string content)
        {
            string Url = "";
            string webcontent = "";
            if (content.Contains("%GETTEXT:"))
            {
                int length = content.LastIndexOf("%GETTEXT");
                string start = content.Substring(content.IndexOf("%GETTEXT"), content.Length - length).Replace(@"\", "");
                string startUrl = start.Split(':')[2];
                Url = start.Split(':')[1] + ":" + startUrl.Split('%')[0];
                WebClient wc = new WebClient();
                webcontent = wc.DownloadString(Url).Replace(@"%", @"\%");
                webcontent = FixString(webcontent);
            }
            string path = "";
            string fileContent = "";
            if (content.Contains("%GETFILE:"))
            {
                int length = content.LastIndexOf("%GETFILE");
                string start = content.Substring(content.IndexOf("%GETFILE"), content.Length - length);
                string startPath = FixString(start.Split(':')[1]);
                path = startPath.Split('%')[0];
                try
                {
                    fileContent = File.ReadAllText(FixString(path.Replace(@"\\", @"\")));
                }
                catch { }
            }

            string newcontent = content
                    .Replace("%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
                    .Replace("%APPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                    .Replace("%COMMONAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))
                    .Replace("%TEMP%", Path.GetTempPath())
                    .Replace("%DESKTOP%", Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                    .Replace("%DOCUMENTS%", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
                    .Replace("%ADMINTOOLS%", Environment.GetFolderPath(Environment.SpecialFolder.AdminTools))
                    .Replace("%PROGRAMFILES%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))
                    .Replace("%PROGRAMFILESX86%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86))
                    .Replace("%PROGRAMS%", Environment.GetFolderPath(Environment.SpecialFolder.Programs))
                    .Replace("%COOKIES%", Environment.GetFolderPath(Environment.SpecialFolder.Cookies))
                    .Replace("%STARTUP%", Environment.GetFolderPath(Environment.SpecialFolder.Startup))
                    .Replace("%STARTUPMENU%", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu))
                    .Replace("%MUSIC%", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic))
                    .Replace("%SYSTEM%", Environment.GetFolderPath(Environment.SpecialFolder.System))
                    .Replace("%WINDOWS%", Environment.GetFolderPath(Environment.SpecialFolder.Windows))
                    .Replace("%USERPROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
                    .Replace("%RESOURCES%", Environment.GetFolderPath(Environment.SpecialFolder.Resources))
                    .Replace("%CURRENTPATH%", Environment.CurrentDirectory)
                    .Replace("%DUCK%", Program.loc + @"\DuckScript\")
                    .Replace("%CURRENTIME%", DateTime.Now.ToString())
                    .Replace("%CURRENTIMEUTC%", DateTime.UtcNow.ToString())
                    .Replace("%USERNAME%", Environment.UserName)
                    .Replace("%USERDOMAINNAME%", Environment.UserDomainName)
                    .Replace("%MACHINENAME%", Environment.MachineName)
                    .Replace($"%GETTEXT:{Url}%", webcontent)
                    .Replace($"%GETFILE:{path}%", fileContent);

            return newcontent;
        }

        public static string FixKeys(string content)
        {
            string newcontent = content;
            newcontent = Regex.Replace(newcontent, "[+^~(){}]", "{$0}");
            newcontent = newcontent
                .Replace(@"\%", "{%}")
                .Replace("%BACKSPACE%", "{BS}")
                .Replace("%BREAK%", "{BREAK}")
                .Replace("%CAPSLOCK%", "{CAPSLOCK}")
                .Replace("%DELETE%", "{DEL}")
                .Replace("%DOWN%", "{DOWN}")
                .Replace("%END%", "{END}")
                .Replace("%ENTER%", "{ENTER}")
                .Replace("%ESC%", "{ESC}")
                .Replace("%HELP%", "{HELP}")
                .Replace("%HOME%", "{HOME}")
                .Replace("%INSERT%", "{INS}")
                .Replace("%LEFT%", "{LEFT}")
                .Replace("%NUMLOCK%", "{NUMLOCK}")
                .Replace("%PGDN%", "{PGDN}")
                .Replace("%PGUP%", "{PGUP}")
                .Replace("%PRTSC%", "{PRTSC}")
                .Replace("%RIGHT%", "{RIGHT}")
                .Replace("%SCROLLLOCK%", "{SCROLLLOCK}")
                .Replace("%TAB%", "{TAB}")
                .Replace("%UP%", "{UP}")
                .Replace("%F1%", "{F1}")
                .Replace("%F2%", "{F2}")
                .Replace("%F3%", "{F3}")
                .Replace("%F4%", "{F4}")
                .Replace("%F5%", "{F5}")
                .Replace("%F6%", "{F6}")
                .Replace("%F7%", "{F7}")
                .Replace("%F8%", "{F8}")
                .Replace("%F9%", "{F9}")
                .Replace("%F10%", "{F10}")
                .Replace("%F11%", "{F11}")
                .Replace("%F12%", "{F12}")
                .Replace("%Fdd%", "{ADD}")
                .Replace("%SUBTRACT%", "{SUBTRACT}")
                .Replace("%MULTIPLY%", "{MULTIPLY}")
                .Replace("%DIVIDE%", "{DIVIDE}");

            return newcontent;
        }

        public static void sendWebHook(string msg, string username, bool error = false)
        {
            if (Settings.webhook != "")
            {
                if (error == false)
                {
                    Post(Settings.webhook, new NameValueCollection()
                    {
                        { "username", username },
                        { "content", msg }
                    });
                }
                else
                {
                    Post(Settings.errorwebhook, new NameValueCollection()
                    {
                        { "username", username },
                        { "content", msg }
                    });
                }
            }
        }

        public static byte[] Post(string uri, NameValueCollection pairs)
        {
            byte[] numArray;
            using (WebClient webClient = new WebClient())
            {
                numArray = webClient.UploadValues(uri, pairs);
            }
            return numArray;
        }

        public static bool IsAntiVirusOff()
        {
            var regkey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            if (regkey != null)
            {
                var subkey = regkey.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection");
                var val = subkey.GetValue("DisableRealtimeMonitoring");
                if (val != null && val is Int32)
                {
                    var value = (int)val;
                    if (value == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public static void AddFileExtension()
        {
            if (!Directory.Exists(Program.loc + @"\DuckScript"))
                Directory.CreateDirectory(Program.loc + @"\DuckScript");

            try
            {
                RegistryKey req = Registry.ClassesRoot.OpenSubKey(".duck");
                string installedVersion = req.GetValue("version").ToString();
                if (installedVersion != Settings.version)
                {
                    try { File.Delete(Program.loc + @"\DuckScript\DuckScript.exe"); } catch { }
                    try { Registry.ClassesRoot.CreateSubKey(".duck").SetValue("version", Settings.version, RegistryValueKind.String); } catch { }
                }
            }
            catch { }

            try { File.Copy(Application.ExecutablePath, Program.loc + @"\DuckScript\DuckScript.exe"); } catch { }
            Registry.ClassesRoot.CreateSubKey(".duck").SetValue("", "DuckScript", RegistryValueKind.String);
            Registry.ClassesRoot.CreateSubKey(".duck").SetValue("version", Settings.version, RegistryValueKind.String);
            Registry.ClassesRoot.CreateSubKey(@"DuckScript\shell\open\command").SetValue("", Program.loc + @"\DuckScript\DuckScript.exe" + " \"%1\" ", RegistryValueKind.String);
            File.WriteAllBytes(Program.loc + @"\DuckScript\DuckScript.ico", Properties.Resource1.icon);
            Registry.ClassesRoot.CreateSubKey(@"DuckScript\DefaultIcon").SetValue("", Program.loc + @"\DuckScript\DuckScript.ico", RegistryValueKind.String);
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(".duck extension installed!");
            Console.ForegroundColor = color;
        }

        public static void CallError(string error)
        {
            if (Settings.errors == "true")
            {
                if (Settings.errorwebhook != "")
                {
                    sendWebHook(error.ToString(), Settings.username);
                }
                else
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(error);
                    Console.ForegroundColor = color;
                }
            }
        }
        #endregion
    }
}
