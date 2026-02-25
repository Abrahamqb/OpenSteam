using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;

namespace OpenSteam.Service
{
    public class Plugins
    {
        public async Task ManagePluginsInstall()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] millenniumInstaller = await client.GetByteArrayAsync("https://github.com/SteamClientHomebrew/Installer/releases/latest/download/MillenniumInstaller-Windows.exe");
                    string installerPath = Path.Combine(Path.GetTempPath(), "MillenniumInstaller.exe");

                    File.WriteAllBytes(installerPath, millenniumInstaller);

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = installerPath,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    };
                    Process p = Process.Start(startInfo);

                    if (p != null)
                    {
                        await Task.Run(() =>
                        {
                            p.WaitForExit();
                        });
                        NotificationWindow win = new NotificationWindow("¡Millennium installed!", 2);
                        win.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical error: " + ex.Message);
            }
        }

        public async Task KernelLuaInstallerAsync(string steamPath)
        {
            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath))
            {
                MessageBox.Show("Steam path not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    catch (Exception)
                    {
                        MessageBox.Show("The KernelLua ZIP file is corrupt. Or it may already exist (Continue to try again)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Thread.Sleep(800);
                        File.Delete(Path.Combine(pluginsFolder, "KernelLua"));
                        return;
                    }

                });


                NotificationWindow win = new NotificationWindow("¡KernelLua successfully installed!", 2);
                win.Show();

                if (File.Exists(tempZip)) File.Delete(tempZip);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
