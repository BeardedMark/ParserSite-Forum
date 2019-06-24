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
using System.Windows.Threading;

namespace PoEParser
{
    /// <summary>
    /// Логика взаимодействия для Alert.xaml
    /// </summary>
    public partial class Alert : Window
    {
        public DispatcherTimer timer = new DispatcherTimer();
        public DispatcherTimer resize = new DispatcherTimer();

        public Alert(string str)
        {
            InitializeComponent();
            double screenHeight = SystemParameters.FullPrimaryScreenHeight;
            double screenWidth = SystemParameters.FullPrimaryScreenWidth;

            this.Top = (screenHeight - this.Height);
            this.Left = (screenWidth - this.Width);

            timer.Tick += new EventHandler(FormClose);
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Start();

            lab1.Content = str;
        }

        void FormClose(object sender, EventArgs e)
        {
            Close();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
