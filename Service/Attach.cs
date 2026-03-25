using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenSteam.Service
{
    public class Attach
    {
        public async Task PatchSteam(string path, bool Delet)
        {
            if (Delet)
            {
                try
                {
                    string JsonPath = Path.Combine(path, "OpenSteamDel.json");
                    if (File.Exists(JsonPath))
                    {
                        string JsonContent = File.ReadAllText(JsonPath);
                        string[] FilesToDelete = JsonSerializer.Deserialize<string[]>(JsonContent);
                        
                        if(FilesToDelete != null)
                        {
                            foreach(string file in FilesToDelete)
                            {
                                string filePath = Path.Combine(path, file);
                                if(File.Exists(filePath))
                                {
                                    File.Delete(filePath);
                                }
                            }
                        }
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

                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "OpenSteamManager");
                        try 
                        {
                            byte[] fileData = await client.GetByteArrayAsync("https://github.com/Abrahamqb/OpenSteamMore-Dev/releases/latest/download/inject.zip");
                            
                            await File.WriteAllBytesAsync(zipPath, fileData);

                            ZipFile.ExtractToDirectory(zipPath, path, true);

                            File.Delete(zipPath);

                            NotificationWindow win = new NotificationWindow("¡Steam Patched!", 2);
                            win.Show();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                    }
                }
            }

        }
    }
}
