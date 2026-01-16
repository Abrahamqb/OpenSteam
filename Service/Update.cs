using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenSteam.Service
{
    public static class Update
    {
        public static string GetVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        public static async Task CheckForUpdates()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "OpenSteamManager");

                    string latestVersionString = await client.GetStringAsync("https://raw.githubusercontent.com/Abrahamqb/OpenSteam/refs/heads/master/version.txt");
                    latestVersionString = latestVersionString.Trim();

                    Version latestVersion = new Version(latestVersionString);
                    Version currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                    if (latestVersion > currentVersion)
                    {
                        MessageBox.Show($"A new version is available: v{latestVersion}\n\nYou are using: v{currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}",
                            "Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking for updates: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
