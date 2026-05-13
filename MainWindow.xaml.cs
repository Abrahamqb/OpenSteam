using OpenSteam.Properties;
using OpenSteam.Service;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenSteam
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (Settings.Default.InitialMessage == true)
            {
                ShowInitialMessage();
            }
            else
            {
                ShowHome();
            }

            State();
            var version = Update.GetVersion();
            txtVersion.Text = $"v{version} | .NET 9 Edition";
            _ = Update.CheckForUpdates();
            _ = Update.GetNews();

            this.Closing += MainWindow_Closing;

            if (Properties.Settings.Default.AutoPatchLaunch)
            {
                Attach attach = new Attach();
                _ = attach.PatchSteam(SteamUtils.GetSteamPath(), false);
                State();
            }

            // Load Settings state
            AutoPatch_.IsChecked = Properties.Settings.Default.AutoPatchLaunch;
            DisableWebHelper_.IsChecked = Properties.Settings.Default.DisableWebHelper;
            CloseSteamPatch_.IsChecked = Properties.Settings.Default.CloseSteamBefore;
            DeleteAutoPatch_.IsChecked = Properties.Settings.Default.DeleteOnClose;
            DisableNFSWAlert_.IsChecked = Properties.Settings.Default.DisableNFSWAlert;
        }
        public void ShowInitialMessage()
        {
            HomeGrid.Visibility = Visibility.Collapsed;
            SettingsGrid.Visibility = Visibility.Collapsed;
            DynamicContent.Visibility = Visibility.Visible;
            DynamicContent.Content = new InitialMessage();
        }

        public void ShowHome()
        {
            DynamicContent.Visibility = Visibility.Collapsed;
            DynamicContent.Content = null;
            HomeGrid.Visibility = Visibility.Visible;
            SettingsGrid.Visibility = Visibility.Collapsed;
        }

        public void State()
        {
            if (File.Exists(Path.Combine(SteamUtils.GetSteamPath(), "xinput1_4.dll")) || File.Exists(Path.Combine(SteamUtils.GetSteamPath(), "hid.dll")) || File.Exists(Path.Combine(SteamUtils.GetSteamPath(), "dwmapi.dll")))
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

        // Navigation Handlers
        private void NavHome_Click(object sender, RoutedEventArgs e)
        {
            HomeGrid.Visibility = Visibility.Visible;
            SettingsGrid.Visibility = Visibility.Collapsed;
            DynamicContent.Visibility = Visibility.Collapsed;
            DynamicContent.Content = null;
        }

        private void NavByPass_Click(object sender, RoutedEventArgs e)
        {
            HomeGrid.Visibility = Visibility.Collapsed;
            SettingsGrid.Visibility = Visibility.Collapsed;
            DynamicContent.Visibility = Visibility.Visible;
            DynamicContent.Content = new OnlineByPass();
        }

        private void NavOnline_Click(object sender, RoutedEventArgs e)
        {
            HomeGrid.Visibility = Visibility.Collapsed;
            SettingsGrid.Visibility = Visibility.Collapsed;
            DynamicContent.Visibility = Visibility.Visible;
            DynamicContent.Content = new OnlineLua();
        }

        private void NavLibrary_Click(object sender, RoutedEventArgs e)
        {
            HomeGrid.Visibility = Visibility.Collapsed;
            SettingsGrid.Visibility = Visibility.Collapsed;
            DynamicContent.Visibility = Visibility.Visible;
            DynamicContent.Content = new LibrarySteam();
        }

        private void NavExtra_Click(object sender, RoutedEventArgs e)
        {
            HomeGrid.Visibility = Visibility.Collapsed;
            SettingsGrid.Visibility = Visibility.Collapsed;
            DynamicContent.Visibility = Visibility.Visible;
            DynamicContent.Content = new Extra();
        }

        private void NavSettings_Click(object sender, RoutedEventArgs e)
        {
            HomeGrid.Visibility = Visibility.Collapsed;
            SettingsGrid.Visibility = Visibility.Visible;
            DynamicContent.Visibility = Visibility.Collapsed;
            DynamicContent.Content = null;
        }

        private void NavInfo_Click(object sender, RoutedEventArgs e)
        {
            HomeGrid.Visibility = Visibility.Collapsed;
            SettingsGrid.Visibility = Visibility.Collapsed;
            DynamicContent.Visibility = Visibility.Visible;
            DynamicContent.Content = new Information();
        }

        // Home Handlers
        private async void patchButton_Click(object sender, RoutedEventArgs e)
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
                            try { proceso.Kill(); } catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            Attach attach = new Attach();
            await attach.PatchSteam(SteamUtils.GetSteamPath(), false);
            State();
        }

        private async void DeletePatchButton_Click(object sender, RoutedEventArgs e)
        {
            Attach attach = new Attach();
            await attach.PatchSteam(SteamUtils.GetSteamPath(), true);
            State();
        }

        private async void Plugins_Click(object sender, RoutedEventArgs e)
        {
            Plugins plugins = new Plugins();
            await plugins.ManagePluginsInstall();
            await Task.Delay(1000);
            await plugins.LuaManagerInstallerAsync(SteamUtils.GetSteamPath());
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

        // Window Controls
        private void Drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        // Settings Handlers
        private void CleanCache_Click(object sender, RoutedEventArgs e) => SettingsFunction.CleanSteamCache();
        private async void SteamBackup_Click(object sender, RoutedEventArgs e) => await SettingsFunction.SteamFolderBackup();
        private void ConfigBackup_Click(object sender, RoutedEventArgs e) => SettingsFunction.BackupSteamConfig();
        private void Folder_Click(object sender, RoutedEventArgs e) => SettingsFunction.OpenFolder();

        private void DeleteAutoPatch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DeleteOnClose = DeleteAutoPatch_.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }

        private void AutoPatch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoPatchLaunch = AutoPatch_.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }

        private void CloseSteamPatch(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CloseSteamBefore = CloseSteamPatch_.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }

        private void DisableWebHelper(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DisableWebHelper = DisableWebHelper_.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }

        private void DisableNFSWAlert(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DisableNFSWAlert = DisableNFSWAlert_.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }
    }
}