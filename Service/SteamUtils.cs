using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSteam.Service
{
    public static class SteamUtils
    {
        public static void Reset() 
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("steam");

                if (processes.Length > 0)
                {
                    foreach (Process proceso in processes)
                    {
                        try
                        {
                            proceso.Kill();
                        }
                        catch { }
                    }
                }
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "steam://flushconfig",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Something went wrong: {ex.Message}");
            }
        }
        public static string GetSteamPath()
        {
            string registryPath = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
            if (registryPath != null)
                return registryPath.Replace("/", "\\");
            string defaultPath = @"C:\Program Files (x86)\Steam";
            if (Directory.Exists(defaultPath)) return defaultPath;
            return null;
        }
    }
}
