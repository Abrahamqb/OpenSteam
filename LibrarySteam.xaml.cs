using OpenSteam.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenSteam
{
    public partial class LibrarySteam : Window
    {
        private string luaPath;
        private string steamPath;
        private static readonly HttpClient client = new HttpClient();

        public LibrarySteam()
        {
            InitializeComponent();

            if (!client.DefaultRequestHeaders.Contains("User-Agent"))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            }

            steamPath = SteamUtils.GetSteamPath();

            if (steamPath != null)
            {
                luaPath = Path.Combine(steamPath, "config", "stplug-in");
                if (!Directory.Exists(luaPath))
                {
                    Directory.CreateDirectory(luaPath);
                }
                _ = RefreshLuaList();
            }
            else
            {
                MessageBox.Show("Steam was not found on this system.");
            }
        }

        private async Task<string> GetGameName(string appId)
        {
            try
            {
                string acfPath = Path.Combine(steamPath, "steamapps", $"appmanifest_{appId}.acf");
                if (File.Exists(acfPath))
                {
                    string content = File.ReadAllText(acfPath);
                    var match = Regex.Match(content, "\"name\"\\s+\"([^\"]+)\"");
                    if (match.Success) return match.Groups[1].Value + " (Not deleting can be native!)";
                }
            }
            catch { }

            try
            {
                string url = $"https://store.steampowered.com/app/{appId}/?l=spanish";
                string html = await client.GetStringAsync(url);

                var titleMatch = Regex.Match(html, "<title>(.*?) en Steam</title>", RegexOptions.IgnoreCase);

                if (titleMatch.Success)
                {
                    string cleanName = titleMatch.Groups[1].Value;
                    if (cleanName.Contains(" en "))
                    {
                        cleanName = cleanName.Split(new[] { " en " }, StringSplitOptions.None).Last();
                    }

                    return cleanName.Trim()+" (OpenSteam)";
                }
            }
            catch 
            {
                return $"Connection error: {appId}.lua";
            }

            return appId;
        }

        private async Task RefreshLuaList()
        {
            LuaListBox.Items.Clear();
            if (!Directory.Exists(luaPath)) return;

            string[] files = Directory.GetFiles(luaPath, "*.lua");

            foreach (string file in files)
            {
                string id = Path.GetFileNameWithoutExtension(file);

                ListBoxItem item = new ListBoxItem
                {
                    Content = $"loading data ({id})...",
                    Tag = Path.GetFileName(file)
                };
                LuaListBox.Items.Add(item);

                _ = Task.Run(async () => {
                    string realName = await GetGameName(id);
                    Dispatcher.Invoke(() => {
                        item.Content = $"{realName}";
                    });
                });
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LuaListBox.SelectedItems.Count == 0) return;

            if (MessageBox.Show("Delete selected files?", "Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (ListBoxItem item in LuaListBox.SelectedItems)
                {
                    string fileName = item.Tag.ToString();
                    string path = Path.Combine(luaPath, fileName);
                    if (File.Exists(path)) File.Delete(path);
                }
                _ = RefreshLuaList();
            }
        }

        private void BtnOpenSteam_Click(object sender, RoutedEventArgs e)
        {
            if (LuaListBox.SelectedItems.Count == 0) return;
            foreach (ListBoxItem item in LuaListBox.SelectedItems)
            {
                var appid = item.Tag.ToString().Replace(".lua", "");
                Process.Start(new ProcessStartInfo($"https://store.steampowered.com/app/{appid}") { UseShellExecute = true });
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}