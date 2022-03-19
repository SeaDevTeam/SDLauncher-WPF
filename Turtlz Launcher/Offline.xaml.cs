using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.VersionLoader;

namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for Offline.xaml
    /// </summary>
    public partial class Offline : Window
    {
        string javaPath;
        MinecraftPath gamepath;
        CMLauncher launcher;
        public Offline()
        {
            InitializeComponent();
            UI(false);
            if (!string.IsNullOrEmpty(MinecraftPath.GetOSDefaultPath()))
            {
                launcher = new CMLauncher(new MinecraftPath(Properties.Settings.Default.MCPath));
            }
            else
            {
                launcher = new CMLauncher(new MinecraftPath(MinecraftPath.GetOSDefaultPath()));
            }
            gamepath = launcher.MinecraftPath;
            launcher.VersionLoader = new LocalVersionLoader(launcher.MinecraftPath);
            launcher.FileDownloader = null;
            refreshVersions();
        }
        void UI(bool val)
        {
            btnLaunch.IsEnabled = val;
            btnMCpath.IsEnabled = val;
            cmbxVer.IsEnabled = val;
        }
        private async void refreshVersions()
        {
            cmbxVer.Items.Clear();
            UI(false);
            var fileslist = Directory.GetFiles(gamepath.Runtime, "*.exe*", SearchOption.AllDirectories);
            javaPath = "";
            bool found = false;
            foreach (var file in fileslist)
            {
                if (file.Contains("javaw.exe"))
                {
                    if (!found)
                    {
                        javaPath = file;
                    }
                    found = true;
                }
            }
            var mcVers = await launcher.GetAllVersionsAsync();
            foreach (var item in mcVers)
            {
                cmbxVer.Items.Add(item.Name);
            }
            UI(true);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private async void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            char[] delimiters = new char[] { ' ', '\r', '\n' };
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Please enter a username!");
                return;
            }
            if (txtName.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length != 1)
            {
                MessageBox.Show("You have to remove " + (txtName.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length - 1).ToString() + " words of your user name");
                return;
            }
            if (string.IsNullOrEmpty(cmbxVer.Text))
            {
                MessageBox.Show("Please enter a version!");
                return;
            }
            var opt = new MLaunchOption();
            opt.Session = MSession.GetOfflineSession(txtName.Text);
            opt.MaximumRamMb = 2048;
            try
            {
                var process = await launcher.CreateProcessAsync(cmbxVer.Text, opt, false);
                MessageBox.Show(process.StartInfo.FileName);
                process.Start();
            }
            catch (KeyNotFoundException)
            {
                MessageBox.Show("Cannot find " + cmbxVer.Text);
            }
            catch (IOException)
            {
                MessageBox.Show("Failed to start version " + cmbxVer.Text + "! You may have already started that version");
            }
            catch (Win32Exception wex) // java exception
            {
                MessageBox.Show(this, wex + "\n\nIt seems your java setting has problem");
            }
        }

        private void btnMCpath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderdia = new System.Windows.Forms.FolderBrowserDialog();
            folderdia.SelectedPath = gamepath.BasePath;
            if (folderdia.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var mc = new MinecraftPath(folderdia.SelectedPath.ToString());
                gamepath = mc;
                launcher = new CMLauncher(gamepath);
                launcher.VersionLoader = new LocalVersionLoader(launcher.MinecraftPath);
                launcher.FileDownloader = null;
                refreshVersions();
            }
        }
    }
}
