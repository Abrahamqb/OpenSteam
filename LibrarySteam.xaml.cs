using OpenSteam.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenSteam
{
    /// <summary>
    /// Lógica de interacción para LibrarySteam.xaml
    /// </summary>
    public partial class LibrarySteam : Window
    {

        private string luaPath;

        public LibrarySteam()
        {
            InitializeComponent();
            string steam = SteamUtils.GetSteamPath();

            if (steam != null)
            {
                luaPath = Path.Combine(steam, "config", "stplug-in");
                if (!Directory.Exists(luaPath))
                {
                    Directory.CreateDirectory(luaPath);
                }
                RefreshLuaList();
            }
            else
            {
                MessageBox.Show("Steam was not found on this system.");
            }
        }

        private void RefreshLuaList()
        {
            LuaListBox.Items.Clear();
            if (Directory.Exists(luaPath))
            {
                string[] files = Directory.GetFiles(luaPath, "*.lua");
                foreach (string file in files)
                {
                    LuaListBox.Items.Add(Path.GetFileName(file));
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LuaListBox.SelectedItems.Count == 0) return;

            var result = MessageBox.Show("Delete selected files?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var item in LuaListBox.SelectedItems)
                {
                    string path = Path.Combine(luaPath, item.ToString());
                    if (File.Exists(path)) File.Delete(path);
                }
                RefreshLuaList();
            }
        }

        private void BtnOpenSteam_Click(object sender, RoutedEventArgs e)
        {
            if (LuaListBox.SelectedItems.Count == 0) return;
            foreach (var item in LuaListBox.SelectedItems)
            {
                var appid = item.ToString().Replace(".lua", "");
                Process.Start(new ProcessStartInfo("https://steamdb.info/app/"+appid) { UseShellExecute = true });
            }
             RefreshLuaList();
        }

        private void Back_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
    }
}
