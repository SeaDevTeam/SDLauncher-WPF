using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Concurrent;
using ModernWpf.Controls;
namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for GameLog.xaml
    /// </summary>
    public partial class GameLog : ContentDialog
    {
        public GameLog()
        {
            InitializeComponent();
        }


        static ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();

        public static void AddLog(string msg)
        {
            logQueue.Enqueue(msg);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string msg;
            while (logQueue.TryDequeue(out msg))
            {
                txtLog.Text += msg + "\n";
            }
        }
    }
}
