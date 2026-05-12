using OpenSteam.Service;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace OpenSteam
{
    public partial class LibrarySteam : UserControl
    {
        private string luaPath;
        private string steamPath;
        private List<Game> fullGameList = new List<Game>();

        public LibrarySteam()
        {
            InitializeComponent();

            steamPath = SteamUtils.GetSteamPath();

            if (steamPath != null)
            {
                luaPath = Path.Combine(steamPath, "config", "stplug-in");
                if (!Directory.Exists(luaPath))
                {
                    Directory.CreateDirectory(luaPath);
                }
                _ = LoadData();
            }
            else
            {
                MessageBox.Show("Steam was not found on this system.");
            }
        }

        private async Task LoadData()
        {
            try
            {
                fullGameList = await SteamUtils.DownloadGameListAsync();
            }
            catch { /* Fallback to empty list if network/cache fails */ }

            await RefreshLuaList();
        }

        private string GetGameNameLocal(string appId)
        {
            // 1. Try local ACF (Native)
            try
            {
                string acfPath = Path.Combine(steamPath, "steamapps", $"appmanifest_{appId}.acf");
                if (File.Exists(acfPath))
                {
                    string content = File.ReadAllText(acfPath);
                    var match = Regex.Match(content, "\"name\"\\s+\"([^\"]+)\"");
                    if (match.Success) return match.Groups[1].Value + " (Good)";
                }
            }
            catch { }

            // 2. Try JSON Cache (Fast)
            var game = fullGameList.FirstOrDefault(g => g.appid == appId);
            if (game != null)
            {
                return game.name + " (Very Good)";
            }

            return appId + ".lua";
        }

        private async Task RefreshLuaList()
        {
            LuaListBox.Items.Clear();
            if (!Directory.Exists(luaPath)) return;

            string[] files = Directory.GetFiles(luaPath, "*.lua");

            foreach (string file in files)
            {
                string id = Path.GetFileNameWithoutExtension(file);
                string realName = GetGameNameLocal(id);

                ListBoxItem item = new ListBoxItem
                {
                    Content = realName,
                    Tag = Path.GetFileName(file)
                };
                LuaListBox.Items.Add(item);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LuaListBox.SelectedItems.Count == 0) return;

            if (MessageBox.Show("Delete selected files?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var itemsToRemove = new List<ListBoxItem>();
                foreach (ListBoxItem item in LuaListBox.SelectedItems)
                {
                    itemsToRemove.Add(item);
                    try
                    {
                        string fileName = item.Tag.ToString();
                        string path = Path.Combine(luaPath, fileName);
                        if (File.Exists(path)) File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting file: {ex.Message}");
                    }
                }

                // Remove from UI without refreshing everything
                foreach (var item in itemsToRemove)
                {
                    LuaListBox.Items.Remove(item);
                }
            }
        }

        private void BtnOpenSteam_Click(object sender, RoutedEventArgs e)
        {
            if (LuaListBox.SelectedItems.Count == 0) return;
            foreach (ListBoxItem item in LuaListBox.SelectedItems)
            {
                var appid = item.Tag.ToString().Replace(".lua", "");
                try
                {
                    Process.Start(new ProcessStartInfo($"https://store.steampowered.com/app/{appid}") { UseShellExecute = true });
                }
                catch { }
            }
        }
    }
}