//using System.Windows.Shapes;
using OpenSteam.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace OpenSteam
{
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            Estado();
        }

        public void Estado()
        {
            if (File.Exists(Path.Combine(GetSteamPath(), "xinput1_4.dll")) && File.Exists(Path.Combine(GetSteamPath(), "hid.dll")))
            {
                ParcheEstado.Text = "Parche instalado";
                StatusDot.Fill = Brushes.LimeGreen;
            }
            else
            {
                ParcheEstado.Text = "Parche no instalado (necesario Parchear)";
                StatusDot.Fill = Brushes.Red;
            }
        }

        private void patchButton_Click(object sender, RoutedEventArgs e)
        {
            attach attach = new attach();
            attach.PatchSteam(GetSteamPath(), false);
            Estado();
        }

        private void DeletePatchButton_Click(object sender, RoutedEventArgs e)
        {
            attach attach = new attach();
            attach.PatchSteam(GetSteamPath(), true);
            Estado();
        }

        private async void Plugins_Click(object sender, RoutedEventArgs e)
        {
            Plugins plugins = new Plugins();
            await plugins.ManagePluginsInstall();
            Thread.Sleep(1000);
            await plugins.KernelLuaInstallerAsync(GetSteamPath());
        }

        private void ManualLua_Click(object sender, RoutedEventArgs e)
        {
            LuaLoaders luaLoaders = new LuaLoaders();
            luaLoaders.Load(GetSteamPath());
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

        private void ResetSteam_Click(object sender, RoutedEventArgs e)
        {
            SteamUtils steamUtils = new SteamUtils();
            steamUtils.Reset();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Information Info = new Information();
            Info.Show();
        }

        private void OnlineLua_Click(object sender, RoutedEventArgs e)
        {
            OnlineLua onlineLua = new OnlineLua();
            onlineLua.Show();
        }

        private void Drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
