using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Net;
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
    /// Interaction logic for aboutPage.xaml
    /// </summary>
    public partial class aboutPage : ContentDialog
    {
        public aboutPage()
        {
            InitializeComponent();
        }
        string Filedir;
        bool exe;
        private void Update(string link, string dir, bool isexe)
        {
            exe = isexe;
            Filedir = dir;
            btnUpdate.IsEnabled = false;
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(link), dir);
            });
            thread.Start();
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate ()
                    {
                        double bytesIn = double.Parse(e.BytesReceived.ToString());
                        double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                        double percentage = bytesIn / totalBytes * 100;
                        pnlUPUI.Visibility = Visibility.Visible;
                        btnUpdate.IsEnabled = false;
                        if (!exe)
                        {
                            state.Text = "Checking for updates";
                        }
                        else
                        {
                        state.Text = "Downloading for update " + Math.Truncate(percentage).ToString() + "%";
                        }
                    }));
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               new Action(
               delegate ()
               {
                   btnUpdate.IsEnabled = true;
                   pnlUPUI.Visibility = Visibility.Collapsed;
                   if (!exe)
                   {
                       updateINI();
                   }
                   else
                   {
                       System.Diagnostics.Process.Start(Filedir);
                       Application.Current.Shutdown();
                   }
               }
               ));
        }

        void updateINI()
        {
            IniReader parser = new IniReader(Filedir);
            string major = parser.GetValue("major");
            string minor = parser.GetValue("minor");
            string build = parser.GetValue("build");
            string full = major + "." + minor + build;
            MessageBox.Show(full);
            double ver = double.Parse(full);
            if (ver > 0.75)
            {
                if(MessageBox.Show("Version " + full + " is available ! Do you want to download and run the installer now ?","Updates available",MessageBoxButton.YesNo,MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Update("https://raw.githubusercontent.com/Chaniru22/Emerald-Launcher-Public/main/updates.exe", System.IO.Directory.GetCurrentDirectory() + "\\updates.exe", true);
                }
            }
            else
            {
                MessageBox.Show("You are up to date");
            }
        }

        private void cml_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/CmlLib/CmlLib.Core");
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Update("https://raw.githubusercontent.com/Chaniru22/Emerald-Launcher-Public/main/updates.ini", System.IO.Directory.GetCurrentDirectory() + "\\updates.ini",false);
        }
    }
}
