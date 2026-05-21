using OpenSteam.Services;
using OpenSteam.Views;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class Attach
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static string GetReleaseDownloadUrl(string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
            return null;
        try
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonText))
            {
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("assets", out JsonElement assets))
                {
                    var releaseAsset = assets.EnumerateArray()
                        .FirstOrDefault(asset => asset.GetProperty("name").GetString().EndsWith("-Release.zip", StringComparison.OrdinalIgnoreCase));
                    if (releaseAsset.ValueKind != JsonValueKind.Undefined)
                    {
                        return releaseAsset.GetProperty("browser_download_url").GetString();
                    }
                }
            }
        }
        catch (JsonException)
        {
            Console.WriteLine("The provided text is not a valid JSON string.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
        return null;
    }

    public async Task PatchSteam(string path, bool Delet)
    {
        if (Delet)
        {
            await SteamUtils.StopSteam();
            await Task.Delay(1000);

            string[] FilesDeleted = new[]
            {
                "xinput1_4.dll",
                "hid.dll",
                "dwmapi.dll",
                "OpenSteamTool.dll"
            };
            try
            {
                foreach (string Files in FilesDeleted)
                {
                    string fullPath = Path.Combine(path, Files);
                    try { File.Delete(fullPath); }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            NotificationWindow win = new NotificationWindow("¡Unpatched Steam!", 2);
            win.Show();
        }
        else
        {
            if (Directory.Exists(path))
            {
                string tempPath = Path.Combine(path, "temp");
                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

                string zipPath = Path.Combine(tempPath, "inject.zip");

                if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                {
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "OpenSteamManager");
                }

                try
                {
                    string apiUrl = "https://api.github.com/repos/OpenSteam001/OpenSteamTool/releases/latest";
                    string jsonResponse = await _httpClient.GetStringAsync(apiUrl);

                    string downloadUrl = GetReleaseDownloadUrl(jsonResponse);

                    if (string.IsNullOrEmpty(downloadUrl))
                    {
                        Console.WriteLine("Error: Could not find the stable -Release.zip link.");
                        return;
                    }

                    byte[] fileData = await _httpClient.GetByteArrayAsync(downloadUrl);
                    await File.WriteAllBytesAsync(zipPath, fileData);

                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                            {
                                string destinationPath = Path.Combine(path, entry.Name);

                                entry.ExtractToFile(destinationPath, overwrite: true);
                            }
                        }
                    }

                    File.Delete(zipPath);

                    NotificationWindow win = new NotificationWindow("¡Steam Patched!", 2);
                    win.Show();
                }
                catch (Exception ex)
                {

                    if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "OpenSteamManager");
                    try
                    {
                        byte[] fileData = await _httpClient.GetByteArrayAsync("https://github.com/Abrahamqb/OpenSteamMore-Dev/releases/latest/download/inject.zip");

                        await File.WriteAllBytesAsync(zipPath, fileData);

                        ZipFile.ExtractToDirectory(zipPath, path, true);

                        File.Delete(zipPath);

                        NotificationWindow win = new NotificationWindow("¡Steam Patched!", 2);
                        win.Show();
                    } catch { }
                }
            }
        }
    }
}