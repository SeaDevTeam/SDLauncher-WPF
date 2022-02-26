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
using ModernWpf.Controls;

namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for openToServ.xaml
    /// </summary>
    public partial class openToServ : ContentDialog
    {
        MCStatusEx stat;
        public static System.Windows.Threading.DispatcherTimer timer;

        public openToServ()
        {
            InitializeComponent();
        }
        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Start();
            MenuItem_Click(menuItemHy, e);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (stat != null)
            {
                if (stat.Online == true)
                {
                    Get.Visibility = Visibility.Collapsed;
                    Off.Visibility = Visibility.Collapsed;
                    De.Visibility = Visibility.Visible;
                    try
                    {
                        Play.Text = stat.Players + "/" + stat.MaxPlayers + " Playing";
                        Ping.Text = " " + stat.Latency + "ms";
                        Ver.Text = stat.Version;
                    }
                    catch
                    {

                    }
                }
                else if (stat.Online == null)
                {
                    De.Visibility = Visibility.Collapsed;
                    Get.Visibility = Visibility.Visible;
                    Off.Visibility = Visibility.Collapsed;
                }
                else
                {
                    De.Visibility = Visibility.Collapsed;
                    Get.Visibility = Visibility.Collapsed;
                    Off.Visibility = Visibility.Visible;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(sender is MenuItem m)
            {
                btnMCServ.Content = m.Header;
                if(m.Header.ToString() == "Hypixel")
                {
                    imgServ.Source = new BitmapImage(new Uri(@"/imgServ/Hypixel.png", UriKind.Relative));
                    txtServ.Text = "mc.hypixel.net";
                    stat = new MCStatusEx("mc.hypixel.net", 25565);
                }
                else if(m.Header.ToString() == "Pika Network")
                {
                    imgServ.Source = new BitmapImage(new Uri(@"/imgServ/pika-network.png", UriKind.Relative));
                    txtServ.Text = "play.pika-network.net";
                    stat = new MCStatusEx("play.pika-network.net", 25565);
                }
                else if(m.Header.ToString() == "Minemen")
                {
                    imgServ.Source = new BitmapImage(new Uri(@"/imgServ/mineman.png", UriKind.Relative));
                    txtServ.Text = "minemen.club";
                    stat = new MCStatusEx("minemen.club", 25565);
                }
                else if(m.Header.ToString() == "Bridger")
                {
                    imgServ.Source = new BitmapImage(new Uri(@"/imgServ/bridger.png", UriKind.Relative));
                    txtServ.Text = "bridger.land";
                    stat = new MCStatusEx("bridger.land", 25565);
                }
                else if(m.Header.ToString() == "Bed Wars Practice")
                {
                    imgServ.Source = new BitmapImage(new Uri(@"/imgServ/bedwarspractice.png", UriKind.Relative));
                    txtServ.Text = "bedwarspractice.club";
                    stat = new MCStatusEx("bedwarspractice.club", 25565);
                }
                else if(m.Header.ToString() == "Turtlz Network")
                {
                    imgServ.Source = new BitmapImage(new Uri(@"/imgServ/turtlz.png", UriKind.Relative));
                    txtServ.Text = "turtlzNetwork.aternos.me";
                    stat = new MCStatusEx("turtlzNetwork.aternos.me", 25565);
                }
            }
        }
    }
}
