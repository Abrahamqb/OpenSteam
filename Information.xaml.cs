using OpenSteam.Service;
using System.Windows;
using System.Windows.Input;

namespace OpenSteam
{
    /// <summary>
    /// Lógica de interacción para Information.xaml
    /// </summary>
    public partial class Information : Window
    {
        public Information()
        {
            InitializeComponent();
            var version = Update.GetVersion();
            InfoVersion.Text = $"v{version} | .NET 9 Edition | Jbrequi (Abrahamqb)";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
