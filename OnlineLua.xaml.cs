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
    /// Lógica de interacción para OnlineLua.xaml
    /// </summary>
    public partial class OnlineLua : Window
    {
        public OnlineLua()
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

        private void PowerKernel_Click(object sender, MouseButtonEventArgs e)
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
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ButtonSearch.IsEnabled = false;
            ButtonSearch.Opacity = 0.2;
            LuaLoaders luaLoaders = new LuaLoaders();
            var response = luaLoaders.OnlineLoad(SearchBox.Text, SteamUtils.GetSteamPath());
            ButtonSearch.IsEnabled = true;
            ButtonSearch.Opacity = 1.0;
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

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
