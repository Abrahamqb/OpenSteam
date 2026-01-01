using System;
using System.Collections.Generic;
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

namespace OpenSteam
{
    /// <summary>
    /// Lógica de interacción para NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        public NotificationWindow(string mensaje, int segundos)
        {
            InitializeComponent();
            TxtMensaje.Text = mensaje;
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(segundos);
            timer.Tick += (s, e) => {
                this.Close();
                timer.Stop();
            };
            timer.Start();
        }
    }
}
