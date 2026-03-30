using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

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

                bool disableWeb = Properties.Settings.Default.DisableWebHelper;

                if (disableWeb)
                {
                    string steamPath = GetSteamPath();
                    string steamExe = Path.Combine(steamPath, "steam.exe");

                    Process.Start(steamExe, "-no-browser +open steam://open/minigameslist");
                }
                else
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "steam://flushconfig",
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
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

        private const string JsonUrl = "https://raw.githubusercontent.com/SteamTools-Team/GameList/refs/heads/main/games.json";
        public static async Task<List<Game>> DownloadGameListAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string jsonContent = await client.GetStringAsync(JsonUrl);

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<Game>>(jsonContent, options);
                }
                catch (Exception ex)
                {
                    return new List<Game>();
                }
            }
        }

        public static List<Game> GetFilteredGames(string searchInput, List<Game> fullGameList)
        {
            if (string.IsNullOrWhiteSpace(searchInput) || fullGameList == null)
                return new List<Game>();

            string cleanInput = searchInput.Trim();
            bool isNumeric = cleanInput.All(char.IsDigit);

            if (isNumeric)
            {
                return fullGameList.Where(g => g.appid == cleanInput).ToList();
            }
            else
            {
                return fullGameList.Where(g => g.name != null &&
                       g.name.Contains(cleanInput, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }
        
        public static async Task<(int updated, int luaUpdated)> FixManifests(string steamPath)
        {
            const string API = "https://api.steamproof.net";
            string pluginDir = Path.Combine(steamPath, "config", "stplug-in");
            string depotCache = Path.Combine(steamPath, "depotcache");

            if (!Directory.Exists(pluginDir))
                throw new DirectoryNotFoundException($"No se encontró la carpeta: {pluginDir}");

            Directory.CreateDirectory(depotCache);

            var luaFiles = Directory.GetFiles(pluginDir, "*.lua");
            var needsUpdateIds = new List<string>();
            var luaData = new Dictionary<string, (string path, string content)>();

            foreach (var file in luaFiles)
            {
                string appId = Path.GetFileNameWithoutExtension(file);
                if (!Regex.IsMatch(appId, @"^\d+$")) continue;

                string content = await File.ReadAllTextAsync(file);

                var depotIds = Regex.Matches(content, @"addappid\((\d+)")
                                    .Select(m => m.Groups[1].Value)
                                    .ToList();

                luaData[appId] = (file, content);

                bool missing = depotIds.Any(d => 
                    !Directory.GetFiles(depotCache, $"{d}_*.manifest").Any()
                );

                if (missing || depotIds.Count == 0)
                    needsUpdateIds.Add(appId);
            }

            if (needsUpdateIds.Count == 0) return (0, 0);

            using HttpClient client = new HttpClient();
            int totalManifestsDownloaded = 0;
            int luaFilesUpdated = 0;

            string idsQuery = string.Join(",", needsUpdateIds);
            string jsonResponse = await client.GetStringAsync($"{API}/apps/depots?ids={idsQuery}");
            using var doc = JsonDocument.Parse(jsonResponse);

            if (!doc.RootElement.TryGetProperty("apps", out var appsArray)) return (0, 0);

            foreach (var app in appsArray.EnumerateArray())
            {
                string appId = app.GetProperty("appId").ToString();
                if (!luaData.ContainsKey(appId)) continue;

                var appInfo = luaData[appId];
                var manifestEntries = new List<string>();

                try 
                {
                    string dlInfoJson = await client.GetStringAsync($"{API}/app/{appId}/manifests/download");
                    using var dlDoc = JsonDocument.Parse(dlInfoJson);
                    
                    if (dlDoc.RootElement.TryGetProperty("manifests", out var manifestList))
                    {
                        foreach (var m in manifestList.EnumerateArray())
                        {
                            string dId = m.GetProperty("depotId").ToString();
                            string mId = m.GetProperty("manifestId").ToString();
                            string dlUrl = m.GetProperty("url").ToString(); 
                            
                            string fileName = $"{dId}_{mId}.manifest";
                            string fullPath = Path.Combine(depotCache, fileName);

                            if (!File.Exists(fullPath))
                            {
                                byte[] data = await client.GetByteArrayAsync(dlUrl);
                                await File.WriteAllBytesAsync(fullPath, data);
                                totalManifestsDownloaded++;
                            }
                        }
                    }
                }
                catch {  }

                var depotsFromApi = app.GetProperty("depots").EnumerateArray();
                foreach (var depot in depotsFromApi)
                {
                    string dId = depot.GetProperty("depotId").ToString();
                    if (depot.TryGetProperty("manifests", out var manifests) && 
                        manifests.TryGetProperty("public", out var pub))
                    {
                        string mId = pub.GetProperty("manifestId").ToString();
                        
                        if (depot.TryGetProperty("maxSize", out var sz) && sz.GetRawText() != "0")
                            manifestEntries.Add($"setManifestid({dId}, \"{mId}\", {sz})");
                        else
                            manifestEntries.Add($"setManifestid({dId}, \"{mId}\")");
                    }
                }

                if (manifestEntries.Count > 0)
                {
                    string cleanContent = appInfo.content;

                    cleanContent = Regex.Replace(cleanContent, @"\r?\n?setManifestid\([^\)]*\);?", "", RegexOptions.IgnoreCase);

                    cleanContent = Regex.Replace(cleanContent, @"(\r?\n-- SteamProof Manifests.*)", "", RegexOptions.Singleline);
                    
                    cleanContent = cleanContent.TrimEnd();

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(cleanContent);
                    sb.AppendLine();
                    sb.AppendLine($"-- SteamProof Manifests (updated {DateTime.UtcNow:yyyy-MM-dd HH:mm UTC})");
                    
                    foreach (var entry in manifestEntries)
                        sb.AppendLine(entry);

                    await File.WriteAllTextAsync(appInfo.path, sb.ToString(), new UTF8Encoding(false));
                    luaFilesUpdated++;
                }
            }

            return (totalManifestsDownloaded, luaFilesUpdated);
        }
    }
}
