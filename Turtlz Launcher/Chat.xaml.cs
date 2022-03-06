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
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : ModernWpf.Controls.ContentDialog
    {
        public Chat()
        {
            if (ww2Discord != null) { ww2Discord.Visibility = Visibility.Visible; if (!string.IsNullOrEmpty(lastsource)) { ww2Discord.CoreWebView2.Navigate(lastsource); } }
            InitializeComponent();
        }
        string lastsource;
        private void ContentDialog_PrimaryButtonClick(ModernWpf.Controls.ContentDialog sender, ModernWpf.Controls.ContentDialogButtonClickEventArgs args)
        {
            if (ww2Discord != null)
            {
                lastsource = ww2Discord.Source.AbsolutePath;
                ww2Discord.Visibility = Visibility.Hidden;
            }
            this.Hide();
        }
    }
}
