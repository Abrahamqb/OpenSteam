using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

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

        public enum ExtraUrlOption
        {
            SteamCMD = 1,
            NLGL = 2,
            CreamInstaller = 3,
            OnlineFix = 4,
            SteamAchievementManager = 5
        }

        public string URL(ExtraUrlOption option)
        {
            switch (option)
            {
                case ExtraUrlOption.SteamCMD:
                    //SteamCMD
                    return "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
                case ExtraUrlOption.NLGL:
                    //NLGL
                    return "https://github.com/onajlikezz/Nightlight-Game-Launcher/releases/tag/NLLauncherV4";
                case ExtraUrlOption.CreamInstaller:
                    //CreamInstaller
                    MessageBox.Show("Redirecting to CreamInstaller GitHub page. It is necessary to compile", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return "https://github.com/CyberSys/CreamInstaller";
                case ExtraUrlOption.OnlineFix:
                    //Online Fix
                    MessageBox.Show("Redirecting to Online-fix.me. You need to log in to download", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return "https://online-fix.me/";
                case ExtraUrlOption.SteamAchievementManager:
                    //Steam Achievement Manager
                    MessageBox.Show("Redirecting to Steam Archievement Manager. This program may lead to a ban if used improperly.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return "https://github.com/gibbed/SteamAchievementManager";
                default:
                    return string.Empty;
            }
        }

        private void OpenExternalUrl(ExtraUrlOption option)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = URL(option),
                    UseShellExecute = true
                });
            }
            catch { /* Exception is intentionally swallowed as per existing pattern */ }
        }

        private void Steamcmd(object sender, MouseButtonEventArgs e)
        {
            OpenExternalUrl(ExtraUrlOption.SteamCMD);
        }

        private void nlgl(object sender, MouseButtonEventArgs e)
        {
            OpenExternalUrl(ExtraUrlOption.NLGL);
        }

        private void craminstaller(object sender, MouseButtonEventArgs e)
        {
            OpenExternalUrl(ExtraUrlOption.CreamInstaller);
        }

        private void onlinefix(object sender, MouseButtonEventArgs e)
        {
            OpenExternalUrl(ExtraUrlOption.OnlineFix);
        }

        private void steamachievementmanager(object sender, MouseButtonEventArgs e)
        {
            OpenExternalUrl(ExtraUrlOption.SteamAchievementManager);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
