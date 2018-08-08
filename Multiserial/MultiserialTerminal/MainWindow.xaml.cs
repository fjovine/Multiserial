using System;
using System.Windows;
using System.Diagnostics;
using System.Text;

namespace MultiserialTerminal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            StringBuilder s = new StringBuilder();
            InitializeComponent();
            SerialFrontend frontend = new SerialFrontend("COM6");

            frontend.ByteAvailable += (o, e) =>
            {
                ;
                s.Append(string.Format("{0:X2} {1} ", e.TheByte, (char)e.TheByte));
//                if (e.TheByte == 0xD)
//                {
//                    s.Append("\n");
//                }
                this.output.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.output.Text = s.ToString();
                }));
            };

            bool ok = frontend.OpenAndDemultipex();
            if (! ok)
            {
                this.output.Text = "Open error";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
