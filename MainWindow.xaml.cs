//using System.Windows.Shapes;
using OpenSteam.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Animation;
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
            _ = Update.GetNews();

            this.Closing += MainWindow_Closing;

            if (Properties.Settings.Default.AutoPatchLaunch)
            {
                Attach attach = new Attach();
                attach.PatchSteam(SteamUtils.GetSteamPath(), false);
                State();
            }

            AutoPatch_.IsChecked = Properties.Settings.Default.AutoPatchLaunch;
            DisableWebHelper_.IsChecked = Properties.Settings.Default.DisableWebHelper;
            CloseSteamPatch_.IsChecked = Properties.Settings.Default.CloseSteamBefore;
            DeleteAutoPatch_.IsChecked = Properties.Settings.Default.DeleteOnClose;
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
            if (Properties.Settings.Default.CloseSteamBefore)
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName("steam");

                    if (processes.Length > 0)
                    {
                        foreach (Process proceso in processes)
                        {
                            try
                            {
                                proceso.Kill();
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
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

        //Settings Configs + Configs Buttons
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            FadeOut(MainMenu, SettingsPanel);
        }
        
        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            FadeOut(SettingsPanel, MainMenu);
        }
        private void FadeOut(FrameworkElement toHide, FrameworkElement toShow)
        {
            DoubleAnimation anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            anim.Completed += (s, ev) =>
            {
                toHide.Visibility = Visibility.Collapsed;
                toShow.Visibility = Visibility.Visible;
                toShow.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)));
            };
            toHide.BeginAnimation(OpacityProperty, anim);
        }

        private void CleanCache_Click(object sender, RoutedEventArgs e)
        {
            SettingsFunction.CleanSteamCache();
        }

        private void ConfigBackup_Click(object sender, RoutedEventArgs e)
        {
            SettingsFunction.BackupSteamConfig();
        }

        private void DownloadFolder_Click(object sender, RoutedEventArgs e)
        {
            SettingsFunction.OpenDownloadFolder();
        }

        private void DeleteAutoPatch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DeleteOnClose = !Properties.Settings.Default.DeleteOnClose;

        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.DeleteOnClose)
            {
                try
                {
                    Attach attach = new Attach();
                    attach.PatchSteam(SteamUtils.GetSteamPath(), true);
                    State();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
        private void AutoPatch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoPatchLaunch = !Properties.Settings.Default.AutoPatchLaunch;

        }

        private void CloseSteamPatch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CloseSteamBefore = !Properties.Settings.Default.CloseSteamBefore;

        }

        private void DisableWebHelper(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DisableWebHelper = !Properties.Settings.Default.DisableWebHelper;

        }
    }
}
