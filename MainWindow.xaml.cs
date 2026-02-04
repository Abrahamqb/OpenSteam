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
            State();
            var version = Update.GetVersion();
            txtVersion.Text = $"v{version} | .NET 9 Edition | Jbrequi (Abrahamqb)";
            _ = Update.CheckForUpdates();
        }

        public void State()
        {
            if (File.Exists(Path.Combine(SteamUtils.GetSteamPath(), "xinput1_4.dll")) && File.Exists(Path.Combine(SteamUtils.GetSteamPath(), "hid.dll")))
            {
                ParcheEstado.Text = "Status: System Ready";
                StatusDot.Fill = Brushes.LimeGreen;
            }
            else
            {
                ParcheEstado.Text = "Status: System Not Ready (You need patch)";
                StatusDot.Fill = Brushes.Red;
            }
        }

        private void patchButton_Click(object sender, RoutedEventArgs e)
        {
            Attach attach = new Attach();
            attach.PatchSteam(SteamUtils.GetSteamPath(), false);
            State();
        }

        private void DeletePatchButton_Click(object sender, RoutedEventArgs e)
        {
            Attach attach = new Attach();
            attach.PatchSteam(SteamUtils.GetSteamPath(), true);
            State();
        }

        private async void Plugins_Click(object sender, RoutedEventArgs e)
        {
            Plugins plugins = new Plugins();
            await plugins.ManagePluginsInstall();
            Thread.Sleep(1000);
            await plugins.KernelLuaInstallerAsync(SteamUtils.GetSteamPath());
        }

        private void ManualLua_Click(object sender, RoutedEventArgs e)
        {
            LuaLoaders luaLoaders = new LuaLoaders();
            luaLoaders.Load(SteamUtils.GetSteamPath());
        }

        private void ResetSteam_Click(object sender, RoutedEventArgs e)
        {
            SteamUtils.Reset();
        }

        private void OnlineLua_Click(object sender, RoutedEventArgs e)
        {
            OnlineLua onlineLua = new OnlineLua();
            onlineLua.ShowDialog();
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

        private void LuaManager_Click(object sender, RoutedEventArgs e)
        {
            LibrarySteam librarySteam = new LibrarySteam();
            librarySteam.ShowDialog();
        }

        private void Extra_Click(object sender, RoutedEventArgs e)
        {
            Extra Extra = new Extra();
            Extra.ShowDialog();
        }

        private void Information_Click(object sender, RoutedEventArgs e)
        {
            Information info = new Information();
            info.ShowDialog();
        }
    }
}
