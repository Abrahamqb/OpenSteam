using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    }
}
