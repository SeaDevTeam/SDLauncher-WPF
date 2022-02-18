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

        }
        MLogin login = new MLogin();
        public static string mail;
        public static string pass;
        private void btnMojangLoging_Click(object sender, RoutedEventArgs e)
        {
            mail = txtbxEmail.Text;
            pass = txtbxPasswrd.Password;
            if (string.IsNullOrWhiteSpace(txtbxEmail.Text) || string.IsNullOrWhiteSpace(txtbxPasswrd.Password))
            {
                MessageBox.Show("Please enter email/password");
                return;
            }
            mojang.IsEnabled = false;
            offline.IsEnabled = false;

            var th = new Thread(new ThreadStart(delegate
            {
                var result = login.Authenticate(mail, pass);
                if (result.Result == MLoginResult.Success)
                {
                    MessageBox.Show("Login Success"); // Success Login

                        Invoke(new Action(() =>
                    {
                        UpdateSession(result.Session);
                    }));
                }
                else
                {
                    MessageBox.Show(result.Result.ToString() + "\n" + result.ErrorMessage); // Failed to login. Show error message

                    offline.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate ()
                    {
                        offline.IsEnabled = true;
                    }
                    ));
                    mojang.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate ()
                    {
                        mojang.IsEnabled = true;
                    }
                    ));

                }
            }));
            th.Start();

        }

        public static void Invoke(Action action)
        {
            action.Invoke();
        }

        private void btnOfflineLog_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtbxOffUsername.Text))
            {
                UpdateSession(MSession.GetOfflineSession(txtbxOffUsername.Text));
            }
            else
            {
                MessageBox.Show("Enter a username");
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            mojang.IsEnabled = false;
            offline.IsEnabled = false;
            var th = new Thread(() =>
                {
                    var result = login.TryAutoLogin();

                    if (result.Result != MLoginResult.Success)
                    {
                        MessageBox.Show($"Failed to AutoLogin : {result.Result}\n{result.ErrorMessage}");

                        offline.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate ()
                        {
                            offline.IsEnabled = true;
                        }
                        ));
                        mojang.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate ()
                        {
                            mojang.IsEnabled = true;
                        }
                        ));
                        return;
                    }

                    MessageBox.Show("Auto Login Success!");
                    offline.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate ()
                    {
                        offline.IsEnabled = true;
                    }
                    ));
                    mojang.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate ()
                    {
                        mojang.IsEnabled = true;
                    }
                    ));
                    UpdateSession(result.Session);
                });
                th.Start();

            
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            mojang.IsEnabled = false;
            offline.IsEnabled = false;

                var th = new Thread(() =>
                {
                    var result = login.TryAutoLoginFromMojangLauncher();

                    if (result.Result != MLoginResult.Success)
                    {
                        MessageBox.Show($"Failed to AutoLogin : {result.Result}\n{result.ErrorMessage}");

                        offline.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate ()
                        {
                            offline.IsEnabled = true;
                        }
                        ));
                        mojang.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate ()
                        {
                            mojang.IsEnabled = true;
                        }
                        ));
                        return;
                    }

                    MessageBox.Show("Auto Login Success!");
                    offline.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate ()
                    {
                        offline.IsEnabled = true;
                    }
                    ));
                    mojang.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate ()
                    {
                        mojang.IsEnabled = true;
                    }
                    ));
                    UpdateSession(result.Session);
                });
                th.Start();


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = login.Signout(txtbxEmail.Text, txtbxPasswrd.Password);
            if (result)
            {
                MessageBox.Show("Success");
                mojang.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Fail");
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
            loginWindow.ShowInTaskbar = false;
            loginWindow.WindowStyle = WindowStyle.ToolWindow; 
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
            loginWindow.WindowStyle = WindowStyle.ToolWindow;
            loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            loginWindow.Title = "Logout";
            loginWindow.ShowLogoutDialog();
        }

        private void txtbxPasswrd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void txtbxOffUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                btnOfflineLog_Click(sender, e);
            }
        }

        private void txtbxEmail_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnMojangLoging_Click(sender, e);
            }

        }
    }
}
