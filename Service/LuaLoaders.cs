using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace OpenSteam.Service
{
    public class LuaLoaders
    {
        public void Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("No se ha detectado la ruta de Steam.");
                return;
            }
            string luaPathSteam = Path.Combine(path, "config", "stplug-in");
            OpenFileDialog luaLoader = new OpenFileDialog
            {
                Filter = "Lua Files|*.lua",
                Title = "Seleccionar Script Lua"
            };

            if (luaLoader.ShowDialog() == true)
            {
                try
                {
                    if (!Directory.Exists(luaPathSteam))
                    {
                        Directory.CreateDirectory(luaPathSteam);
                    }

                    string destinationFile = Path.Combine(luaPathSteam, luaLoader.SafeFileName);
                    File.Copy(luaLoader.FileName, destinationFile, true);
                    NotificationWindow win = new NotificationWindow("¡Lua cargado con éxito!", 2);
                    win.Show();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Error: No tienes permisos para escribir en la carpeta de Steam. Ejecuta como administrador.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Algo salió mal: {ex.Message}");
                }
            }
        }

        public async Task OnlineLoad(string ID, string path)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "OpenSteam-Manager/1.0");

                string checkUrl = $"https://kernelos.org/games/download.php?gen=1&id={ID}";
                string luaPathSteam = Path.Combine(path, "config", "stplug-in");
                string tempZip = Path.Combine(Path.GetTempPath(), $"Lua_{ID}.zip");

                try
                {
                    string content = await client.GetStringAsync(checkUrl);
                    using JsonDocument doc = JsonDocument.Parse(content);

                    string pathGameLua = doc.RootElement.GetProperty("url").GetString() ?? "";
                    string fullLink = "https://kernelos.org" + pathGameLua;

                    byte[] zipBytes = await client.GetByteArrayAsync(fullLink);
                    await File.WriteAllBytesAsync(tempZip, zipBytes);

                    await Task.Run(() =>
                    {
                        if (!Directory.Exists(luaPathSteam))
                            Directory.CreateDirectory(luaPathSteam);

                        string finalLuaFile = Path.Combine(luaPathSteam, $"{ID}.lua");

                        string extractPath = Path.Combine(Path.GetTempPath(), "Extract_" + ID);
                        if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);

                        ZipFile.ExtractToDirectory(tempZip, extractPath);

                        string[] files = Directory.GetFiles(extractPath, "*.lua", SearchOption.AllDirectories);

                        if (files.Length > 0)
                        {
                            if (File.Exists(finalLuaFile)) File.Delete(finalLuaFile);
                            File.Move(files[0], finalLuaFile);
                        }

                        Directory.Delete(extractPath, true);
                    });

                    NotificationWindow win = new NotificationWindow($"¡Script {ID}.lua instalado!", 2);
                    win.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error en la instalación: {ex.Message}", "Fallo de red/archivo");
                }
                finally
                {
                    if (File.Exists(tempZip)) File.Delete(tempZip);
                }
            }
        }

    }
}
