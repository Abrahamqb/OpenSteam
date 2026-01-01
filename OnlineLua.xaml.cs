using OpenSteam.Service;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace OpenSteam
{
    /// <summary>
    /// Lógica de interacción para OnlineLua.xaml
    /// </summary>
    public partial class OnlineLua : Window
    {
        public OnlineLua()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonSearch.IsEnabled = false;
            ButtonSearch.Opacity = 0.2;
            LuaLoaders luaLoaders = new LuaLoaders();
            var response = luaLoaders.OnlineLoad(SearchBox.Text, GetSteamPath());
            ButtonSearch.IsEnabled = true;
            ButtonSearch.Opacity = 1.0;
        }
        public string GetSteamPath()
        {
            string registryPath = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
            if (registryPath != null)
                return registryPath.Replace("/", "\\");
            string defaultPath = @"C:\Program Files (x86)\Steam";
            if (Directory.Exists(defaultPath)) return defaultPath;
            return null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
