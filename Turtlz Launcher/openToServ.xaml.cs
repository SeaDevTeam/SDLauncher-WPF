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
        bool iscus = false;

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
                if (!iscus)
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
                else
                {
                    if (stat.Online == true)
                    {
                        CGet.Visibility = Visibility.Collapsed;
                        COff.Visibility = Visibility.Collapsed;
                        CDe.Visibility = Visibility.Visible;
                        try
                        {
                            CPlay.Text = stat.Players + "/" + stat.MaxPlayers + " Playing";
                            CPing.Text = " " + stat.Latency + "ms";
                            CVer.Text = stat.Version;
                        }
                        catch
                        {

                        }
                    }
                    else if (stat.Online == null)
                    {
                        CDe.Visibility = Visibility.Collapsed;
                        CGet.Visibility = Visibility.Visible;
                        COff.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        CDe.Visibility = Visibility.Collapsed;
                        CGet.Visibility = Visibility.Collapsed;
                        COff.Visibility = Visibility.Visible;
                    }

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
                if(m.Header.ToString() == "Custom")
                {
                    stat = null;
                    btnServCusOK.Visibility = Visibility.Visible;
                    btnServOk.Visibility = Visibility.Collapsed;
                    iscus = true;
                }
                else
                {
                    iscus = false;
                    btnServCusOK.Visibility = Visibility.Collapsed;
                    btnServOk.Visibility = Visibility.Visible;
                }
            }
        }

        private void txtxbxServ_TextChanged(object sender, TextChangedEventArgs e)
        {
            stat = new MCStatusEx(txtxbxServ.Text, 25565);
        }

        private void btnServCusOK_Click(object sender, RoutedEventArgs e)
        {
            vars.Serv = txtxbxServ.Text;
            stat = null;
            this.Hide();
        }

        private void btnServOk_Click(object sender, RoutedEventArgs e)
        {
            vars.Serv = txtServ.Text;
            stat = null;
            this.Hide();
        }
    }
}
