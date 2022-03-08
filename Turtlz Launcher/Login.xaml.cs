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
using CmlLib.Core.Auth;
using System.Threading;
using System.Net;
using CmlLib.Core.Auth.Microsoft.UI.Wpf;

namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : ContentDialog
    {
        public Login()
        {
            InitializeComponent();
            startup();
        }
        MLogin login = new MLogin();
        public static string mail;
        public static string pass;
        void startup()
        {
            if (Properties.Settings.Default.autologin)
            {
                if (Properties.Settings.Default.session != null)
                {
                    txtbxOffUsername.Text = Properties.Settings.Default.session.Username;
                }
            }
        }

        public static void Invoke(Action action)
        {
            action.Invoke();
        }

        private void btnOfflineLog_Click(object sender, RoutedEventArgs e)
        {
            char[] delimiters = new char[] { ' ', '\r', '\n' };
            if (!string.IsNullOrEmpty(txtbxOffUsername.Text))
            {
                if (txtbxOffUsername.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length == 1)
                {
                    UpdateSession(MSession.GetOfflineSession(txtbxOffUsername.Text.Replace(" ", "").ToString()));
                }
                else
                {
                    MessageBox.Show("You have to remove " + (txtbxOffUsername.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length - 1).ToString() + " words of your user name");
                }
            }
            else
            {
                MessageBox.Show("Enter a username");
            }
        }
        private void UpdateSession(MSession session)
        {
            // Success to login!
            vars.session = session;
            vars.UserName = session.Username.ToString();
            this.Hide();
        }
        private bool CheckConnection(String URL)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Timeout = 10000;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }


        private void btnMSLogin_Click(object sender, RoutedEventArgs e)
        {
            MicrosoftLoginWindow loginWindow = new MicrosoftLoginWindow();
            loginWindow.Title = "Login with Microsoft Account";
            loginWindow.Height = 800;
            loginWindow.LoadingText = "Loading";
            loginWindow.ShowInTaskbar = false;
            loginWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            loginWindow.ResizeMode = ResizeMode.NoResize;
            loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            MSession session = loginWindow.ShowLoginDialog();
            if (session != null)
            {
                MessageBox.Show("Login success : " + session.Username);
                UpdateSession(session);
            }
            else
            {
                MessageBox.Show("Failed to login");
            }
        }

        private void btnMSLogout_Click(object sender, RoutedEventArgs e)
        {

            MicrosoftLoginWindow loginWindow = new MicrosoftLoginWindow();
            loginWindow.Height = 800;
            loginWindow.ShowInTaskbar = false;
            loginWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            loginWindow.ResizeMode = ResizeMode.NoResize;
            loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            loginWindow.LoadingText = "Loading";
            loginWindow.Title = "Logout";
            loginWindow.ShowLogoutDialog();
        }

        private void txtbxPasswrd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void txtbxOffUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnOfflineLog_Click(sender, e);
            }
        }

    }
}
