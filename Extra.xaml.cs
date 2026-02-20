using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace OpenSteam
{
    /// <summary>
    /// Lógica de interacción para Extra.xaml
    /// </summary>
    public partial class Extra : Window
    {
        public Extra()
        {
            InitializeComponent();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public string URL(int option)
        {
            switch (option)
            {
                case 1:
                    //SteamCMD
                    return "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
                case 2:
                    //NLGL
                    return "https://github.com/onajlikezz/Nightlight-Game-Launcher/releases/tag/NLLauncherV4";
                case 3:
                    //CreamInstaller
                    MessageBox.Show("Redirecting to CreamInstaller GitHub page. It is necessary to compile", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return "https://github.com/CyberSys/CreamInstaller";
                case 4:
                    //Online Fix
                    MessageBox.Show("Redirecting to Online-fix.me. You need to log in to download", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return "https://online-fix.me/";
                case 5:
                    //Steam Achievement Manager
                    MessageBox.Show("Redirecting to Steam Archievement Manager. This program may lead to a ban if used improperly.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return "https://github.com/gibbed/SteamAchievementManager";
                default:
                    return string.Empty;
            }
        }

        private void Steamcmd(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = URL(1),
                    UseShellExecute = true
                });
            } catch{ }
        }

        private void nlgl(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = URL(2),
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void craminstaller(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = URL(3),
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void onlinefix(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = URL(4),
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void steamachievementmanager(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = URL(5),
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
