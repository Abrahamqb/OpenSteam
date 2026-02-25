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
using System.Diagnostics;

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
            } catch (Exception ex)
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

        /*private void PowerKernel_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://kernelos.org/",
                        UseShellExecute = true
                    });
                }
                catch
                {

                }
            }
        }*/

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            LuaLoaders luaLoaders = new LuaLoaders();
            string steamPath = SteamUtils.GetSteamPath();

            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                MessageBox.Show("Please enter an AppID first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ButtonSearch.IsEnabled = false;
            ButtonSearch.Opacity = 0.6;
            ButtonText.Visibility = Visibility.Collapsed;
            ButtonProgress.Visibility = Visibility.Visible;

            try
            {
                string userInput = SearchBox.Text;

                var results = SteamUtils.GetFilteredGames(userInput, CachedList);

                if (results == null || !results.Any())
                {
                    MessageBox.Show("No games found with that ID or Name.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;   
                }

                var selectedGame = results.First();

                Console.WriteLine($"Processing: {selectedGame.name}");
                Console.WriteLine($"ID: {selectedGame.appid} | NSFW: {selectedGame.nsfw} | DRM: {selectedGame.drm}");

                if (selectedGame.nsfw)
                {

                    if (MessageBoxResult.No == MessageBox.Show("This game is marked as NSFW. The Lua file may contain inappropriate content. Do you want to continue?", "NSFW Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                    {
                        return;
                    }
                }
                if(selectedGame.drm)
                {
                    if (MessageBoxResult.No == MessageBox.Show("This game has DRM. The Lua file may not work correctly or may require an external bypass. Do you want to continue?", "DRM Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                    {
                        return;
                    }

                }


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

        private void ExitButton_Click(object sender, RoutedEventArgs e){this.Close();}
    }
}
