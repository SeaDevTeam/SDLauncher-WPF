using Microsoft.Web.WebView2.Core;
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

namespace Turtlz_Launcher
{
    public partial class Chat : ModernWpf.Controls.ContentDialog
    {
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        public Chat()
        {

            InitializeComponent();
            
            timer.Start();
            timerstate = true;
        }
        string lastsource;
        private void ContentDialog_PrimaryButtonClick(ModernWpf.Controls.ContentDialog sender, ModernWpf.Controls.ContentDialogButtonClickEventArgs args)
        {
            if (ww2Discord != null)
            {
                lastsource = ww2Discord.Source.AbsolutePath;
            }
            this.Hide();
        }
        bool timerstate = false;
        //private void ContentDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    timer.Tick += Timer1_Tick;
        //    timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
        //    if (this.Visibility == Visibility.Visible)
        //    {
        //        if (!timerstate)
        //        {
        //            timer.Start();
        //            timerstate = true;
        //        }
        //    }
        //    else
        //    {
        //        ww2Discord.Visibility = Visibility.Hidden;
        //        if (timerstate)
        //        {
        //            timer.Stop();
        //            timerstate = false;
        //        }
        //    }
        //}

        //private void Timer1_Tick(object sender, EventArgs e)
        //{
        //}

        private void ww2Discord_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            ww2Discord.ExecuteScriptAsync("document.addEventListener('contextmenu', event => event.preventDefault());");
            ww2Discord.Visibility = Visibility.Visible;
            ring.Visibility = Visibility.Hidden;
        }
    }
}
