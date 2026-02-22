using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace OpenSteam.Service
{
    public static class SettingsFunction
    {
        private static string GetSteamPath()
        {
            return SteamUtils.GetSteamPath();
        }

        public static void CleanSteamCache()
        {
            string steamPath = GetSteamPath();
            if (string.IsNullOrEmpty(steamPath)) return;

            string appCache = Path.Combine(steamPath, "appcache");

            try
            {
                if (Directory.Exists(appCache))
                {
                    Directory.Delete(appCache, true);
                    MessageBox.Show("Cache folder deleted. Restart Steam to take effect.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cleaning cache: {ex.Message}");
            }
        }

        public static void OpenDownloadFolder()
        {
            string steamPath = GetSteamPath();
            string appsPath = Path.Combine(steamPath, "steamapps", "common");

            if (Directory.Exists(appsPath))
                Process.Start("explorer.exe", appsPath);
        }

        public static void BackupSteamConfig()
        {
            string steamPath = GetSteamPath();
            string configSource = Path.Combine(steamPath, "config");
            string backupDest = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup_Config");

            try
            {
                if (Directory.Exists(configSource))
                {
                    if (!Directory.Exists(backupDest)) Directory.CreateDirectory(backupDest);

                    foreach (string file in Directory.GetFiles(configSource))
                    {
                        File.Copy(file, Path.Combine(backupDest, Path.GetFileName(file)), true);
                    }
                    MessageBox.Show("Config backup created successfully!");
                    Process.Start("explorer.exe", backupDest);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}");
            }
        }
    }
}