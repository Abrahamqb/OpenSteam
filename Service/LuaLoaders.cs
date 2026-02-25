using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;

namespace OpenSteam.Service
{
    public class LuaLoaders
    {
        public void Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("The Steam path was not detected.");
                return;
            }
            string luaPathSteam = Path.Combine(path, "config", "stplug-in");
            OpenFileDialog luaLoader = new OpenFileDialog
            {
                Filter = "Lua Files|*.lua",
                Title = "select Lua"
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
                    NotificationWindow win = new NotificationWindow("¡Lua successfully loaded!", 2);
                    win.Show();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Error: You do not have permission to write to the Steam folder. Run as administrator.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Something went wrong: {ex.Message}");
                }
            }
        }

        //Old option explained in case the new one stops working
        /*public async Task OnlineLoad(string ID, string path)
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

                    NotificationWindow win = new NotificationWindow($"¡Script {ID}.lua successfully loaded!", 2);
                    win.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Something went wrong: {ex.Message}", "Error");
                }
                finally
                {
                    if (File.Exists(tempZip)) File.Delete(tempZip);
                }
            }
        }*/

        public async Task OnlineLoad(string ID, string path)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "OpenSteam-Manager/1.0");

                string luaPathSteam = Path.Combine(path, "config", "stplug-in");
                string ManifestPathSteam = Path.Combine(path, "depotcache");
                string tempZip = Path.Combine(Path.GetTempPath(), $"Lua_{ID}.zip");

                try
                {
                    string fullLink = "https://codeload.github.com/SteamAutoCracks/ManifestHub/zip/refs/heads/" + ID;
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

                        string FinalExtractedFolder = Directory.GetDirectories(extractPath).FirstOrDefault() ?? extractPath;

                        string[] Manifest = Directory.GetFiles(FinalExtractedFolder, "*.manifest", SearchOption.AllDirectories);
                        string[] files = Directory.GetFiles(FinalExtractedFolder, "*.lua", SearchOption.AllDirectories);

                        if (files.Length > 0)
                        {
                            if (File.Exists(finalLuaFile)) File.Delete(finalLuaFile);
                            File.Move(files[0], finalLuaFile);
                        }
                        if (Manifest.Length > 0)
                        {
                            foreach (string manifest in Manifest)
                            {
                                string destManifest = Path.Combine(ManifestPathSteam, Path.GetFileName(manifest));
                                if (File.Exists(destManifest)) File.Delete(destManifest);
                                File.Move(manifest, destManifest);
                            }
                        }
                        Directory.Delete(extractPath, true);
                    });

                    NotificationWindow win = new NotificationWindow($"¡Lua and Manifest successfully loaded!", 2);
                    win.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Something went wrong: {ex.Message}", "Error");
                }
                finally
                {
                    if (File.Exists(tempZip)) File.Delete(tempZip);
                }
            }
        }

    }
}
