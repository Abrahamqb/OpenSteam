using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSteam.Service
{
    public class SteamUtils
    {
        public void Reset() 
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
                System.Windows.MessageBox.Show($"Error al reiniciar Steam: {ex.Message}");
            }
        }
    }
}
