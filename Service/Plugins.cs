using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
//using static System.Net.WebRequestMethods;

namespace OpenSteam.Service
{
    public class Plugins
    {
        public void ManagePluginsInstall()
        {
            try
            {
                string command = "iwr -useb 'https://steambrew.app/install.ps1' | iex";

                byte[] commandBytes = System.Text.Encoding.Unicode.GetBytes(command);
                string encodedCommand = Convert.ToBase64String(commandBytes);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encodedCommand}",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    Verb = "runas"
                };

                using (Process proceso = Process.Start(psi))
                {
                    proceso.WaitForExit();

                    if (proceso.ExitCode == 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            NotificationWindow win = new NotificationWindow("¡Instalando Millennium!", 2);
                            win.Show();
                        });
                    }
                    else
                    {
                        MessageBox.Show($"El proceso terminó con código: {proceso.ExitCode}. Intenta ejecutar como administrador.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error crítico: " + ex.Message);
            }
        }

        public async Task KernelLuaInstallerAsync(string steamPath)
        {
            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath))
            {
                MessageBox.Show("No se encontró la ruta de Steam.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var pluginsFolder = Path.Combine(steamPath, "plugins");
            string tempZip = Path.Combine(Path.GetTempPath(), "KernelLua_Temp.zip");
            string targetPluginDir = Path.Combine(pluginsFolder, "KernelLua");

            try
            {
                await Task.Run(() =>
                {
                    if (!Directory.Exists(pluginsFolder)) Directory.CreateDirectory(pluginsFolder);

                    File.WriteAllBytes(tempZip, Properties.Resources.KernelLua);


                    if (Directory.Exists(targetPluginDir))
                    {
                        Directory.Delete(targetPluginDir, true);
                    }

                    try 
                    {
                        ZipFile.ExtractToDirectory(tempZip, pluginsFolder);
                    }
                    catch(Exception)
                    {
                        MessageBox.Show("El archivo ZIP de KernelLua está corrupto. O Es posible que ya exista (Continua para volver a intentar)", "Error de extracción", MessageBoxButton.OK, MessageBoxImage.Error);
                        Thread.Sleep(800);
                        File.Delete(Path.Combine(pluginsFolder, "KernelLua"));
                        return;
                    }

                });


                NotificationWindow win = new NotificationWindow("¡KernelLua instalado con éxito!", 2);
                win.Show();

                if (File.Exists(tempZip)) File.Delete(tempZip);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico: {ex.Message}", "Fallo de instalación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
