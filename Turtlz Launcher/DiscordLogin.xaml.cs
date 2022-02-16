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
using System.Windows.Shapes;
using System.Web;

namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for DiscordLogin.xaml
    /// </summary>
    public partial class DiscordLogin : Window
    {
        public DiscordLogin()
        {
            InitializeComponent();
            ww2.Source = new Uri("https://discord.com/api/oauth2/authorize?client_id=943158277678714900&redirect_uri=http%3A%2F%2Fhimathsuniverse.rf.gd%2F&response_type=code&scope=rpc");
        }

        private void ww2_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if(ww2.Source != new Uri("https://discord.com/api/oauth2/authorize?client_id=943158277678714900&redirect_uri=http%3A%2F%2Fhimathsuniverse.rf.gd%2F&response_type=code&scope=rpc"))
            {
                try
                {
                    var q = HttpUtility.ParseQueryString(ww2.Source.Query);
                    vars.DiscordToken = q["code"];
                    this.Close();
                }
                catch(Exception ex)
                {
                    vars.DiscordToken = "";

                }
            }
        }
    }
}
