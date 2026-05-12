using OpenSteam.Service;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace OpenSteam
{
    /// <summary>
    /// </summary>
    public partial class OnlineLua : Window
    {
        public OnlineLua()
        {
            InitializeComponent();
            LoadData();
        }

        private List<Game> CachedList = new List<Game>();

        private async void LoadData()
        {
            ButtonSearch.IsEnabled = false;
            ButtonSearch.Opacity = 0.6;
            ButtonText.Visibility = Visibility.Collapsed;
            ButtonProgress.Visibility = Visibility.Visible;

            try
            {
                CachedList = await SteamUtils.DownloadGameListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ButtonSearch.IsEnabled = true;
                ButtonSearch.Opacity = 1.0;
                ButtonText.Visibility = Visibility.Visible;
                ButtonProgress.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            string userInput = SearchBox.Text;

            if (string.IsNullOrWhiteSpace(userInput))
            {
                MessageBox.Show("Please enter an AppID or Name first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ButtonSearch.IsEnabled = false;
            ButtonSearch.Opacity = 0.6;
            ButtonText.Visibility = Visibility.Collapsed;
            ButtonProgress.Visibility = Visibility.Visible;

            try
            {

                var results = await Task.Run(() => SteamUtils.GetFilteredGames(userInput, CachedList));

                if (results == null || !results.Any())
                {
                    MessageBox.Show("No games found with that ID or Name.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Game selectedGame = results.First();


                if (selectedGame.nsfw && !Properties.Settings.Default.DisableNFSWAlert)
                {
                    var res = MessageBox.Show("This game is marked as NSFW. Continue?", "NSFW Warning",
                                             MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.No) return;
                }

                if (selectedGame.drm)
                {
                    var res = MessageBox.Show("This game has DRM. It may not work. Continue?", "DRM Warning",
                                             MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.No) return;
                }


                LuaLoaders luaLoaders = new LuaLoaders();
                string steamPath = SteamUtils.GetSteamPath();
                await luaLoaders.OnlineLoad(selectedGame.appid, steamPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {

                ButtonSearch.IsEnabled = true;
                ButtonSearch.Opacity = 1.0;
                ButtonText.Visibility = Visibility.Visible;
                ButtonProgress.Visibility = Visibility.Collapsed;
            }
        }

        private void Fix65432(object sender, RoutedEventArgs e)
        {
            MessageBoxResult YN = MessageBox.Show("Next, you will be redirected to a Github page and a YouTube video (I am not the owner) that will explain how to use Steamless to fix it.\n I am not responsible for any harm or damage that Steamless may cause. I only made sure it worked.", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            if (YN == MessageBoxResult.OK)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/atom0s/Steamless/releases/tag/v3.1.0.5",
                        UseShellExecute = true
                    });
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://www.youtube.com/watch?v=Izcsmc6ZAxQ",
                        UseShellExecute = true
                    });
                }
                catch
                {

                }
            }
            else
                return;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) { this.Close(); }
    }
}
