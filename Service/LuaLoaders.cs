using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        public static async Task<string> SteamLuaGenerator(int appId, string path, int cacheDays = 7)
        {
            using var http = new HttpClient();

            string apiUrl = $"https://api.steamproof.net/apps/depots?ids={appId}";
            string apiJson = await http.GetStringAsync(apiUrl);

            string cacheDir = Path.Combine(path, "cache");
            Directory.CreateDirectory(cacheDir);

            string cacheFile = Path.Combine(cacheDir, "depotkeys.json");
            string keysJson;
            string keysUrl = "https://gitlab.com/steamautocracks/manifesthub/-/raw/main/depotkeys.json";

            bool shouldRefresh = true;

            if (File.Exists(cacheFile))
            {
                DateTime lastWriteUtc = File.GetLastWriteTimeUtc(cacheFile);
                bool isExpired = lastWriteUtc < DateTime.UtcNow.AddDays(-cacheDays);

                if (!isExpired)
                {
                    keysJson = await File.ReadAllTextAsync(cacheFile, Encoding.UTF8);
                    shouldRefresh = false;
                }
                else
                {
                    keysJson = string.Empty;
                }
            }
            else
            {
                keysJson = string.Empty;
            }

            if (shouldRefresh)
            {
                keysJson = await http.GetStringAsync(keysUrl);
                await File.WriteAllTextAsync(cacheFile, keysJson, Encoding.UTF8);
            }

            var depotKeys = JsonSerializer.Deserialize<Dictionary<string, string>>(keysJson)
                            ?? new Dictionary<string, string>();

            using var doc = JsonDocument.Parse(apiJson);

            var apps = doc.RootElement.GetProperty("apps");
            if (apps.GetArrayLength() == 0)
                throw new Exception("La API no devolvió apps.");

            var app = apps[0];
            var depots = app.GetProperty("depots");

            var sb = new StringBuilder();
            sb.AppendLine($"-- Open Steam Lua Generator {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} --");
            sb.AppendLine($"addappid({appId})");

            foreach (var depot in depots.EnumerateArray())
            {
                int depotId = depot.GetProperty("depotId").GetInt32();
                string depotIdKey = depotId.ToString();

                if (depotKeys.TryGetValue(depotIdKey, out var depotKey) && !string.IsNullOrWhiteSpace(depotKey))
                    sb.AppendLine($"addappid({depotId},1,\"{depotKey}\")");
                else
                    sb.AppendLine($"addappid({depotId},0,\"\")");

                if (depot.TryGetProperty("manifests", out var manifests) &&
                    manifests.ValueKind == JsonValueKind.Object)
                {
                    string? manifestId = null;

                    if (manifests.TryGetProperty("public", out var publicManifest) &&
                        publicManifest.TryGetProperty("manifestId", out var publicManifestId))
                    {
                        manifestId = publicManifestId.GetString();
                    }
                    else
                    {
                        foreach (var branch in manifests.EnumerateObject())
                        {
                            if (branch.Value.TryGetProperty("manifestId", out var anyManifestId))
                            {
                                manifestId = anyManifestId.GetString();
                                if (!string.IsNullOrWhiteSpace(manifestId))
                                    break;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(manifestId))
                        sb.AppendLine($"setManifestid({depotId},\"{manifestId}\")");
                }
            }

            string outputFile = Path.Combine(path, $"{appId}.lua");
            await File.WriteAllTextAsync(outputFile, sb.ToString(), Encoding.UTF8);

            return sb.ToString();
        }
        
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
                    int appid = int.Parse(ID);
                    var lua = await SteamLuaGenerator(appid, luaPathSteam);
                    /*string fullLink = "https://codeload.github.com/SteamAutoCracks/ManifestHub/zip/refs/heads/" + ID;
                    byte[] zipBytes = await client.GetByteArrayAsync(fullLink);
                    await File.WriteAllBytesAsync(tempZip, zipBytes);

                    await Task.Run(() =>
                    {
                        if (!Directory.Exists(luaPathSteam))
                            Directory.CreateDirectory(luaPathSteam);

                        if (!Directory.Exists(ManifestPathSteam))
                            Directory.CreateDirectory(ManifestPathSteam);

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
                    });*/

                    var result = await SteamUtils.FixManifests(path);

                    NotificationWindow win = new NotificationWindow(
                        $"✔ Lua & Manifest loaded",
                        3
                    );
                    win.Show();

                    await Task.Delay(1000);

                    SteamUtils.Reset();
                }
                catch (Exception ex)
                {
                    var result = await SteamUtils.FixManifests(path);
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
