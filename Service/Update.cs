using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Windows;

namespace OpenSteam.Service
{
    internal class Update
    {
        public string GetVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        public void CheckForUpdates()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string latestVersionString = client.DownloadString("https://raw.githubusercontent.com/Abrahamqb/OpenSteam/refs/heads/master/version.txt").Trim();
                    Version latestVersion = new Version(latestVersionString);
                    Version currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    if (latestVersion > currentVersion)
                    {
                        MessageBox.Show($"A new version is available: v{latestVersion}. You are currently using v{currentVersion}. Please visit the official website to download the latest version.", "Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
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
