using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CmlLib;
using System.ComponentModel;
using CmlLib.Core;
using System.Net;
using CmlLib.Core.Auth;
using CmlLib.Core.Downloader;
using System.Diagnostics;
using CmlLib.Core.Installer;
using CmlLib.Core.Files;
using System.IO;

namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var logindialog = new Login();
            logindialog.IsPrimaryButtonEnabled = false;
            logindialog.IsSecondaryButtonEnabled = false;
            logindialog.ShowAsync();
            Window_Loaded();
        }

        CMLauncher launcher;
        readonly MSession session;
        MinecraftPath gamepath;
        string javapath;

        GameLog logPage;

        private async void Window_Loaded()
        {
            var defaultpath = new MinecraftPath(MinecraftPath.GetOSDefaultPath());
            await initializeLauncher(defaultpath);
        }
        // Event Handler. Show download progress
        private void Launcher_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Pb_Progress.Maximum = 100;
            Pb_Progress.Value = e.ProgressPercentage;
        }

        private void Launcher_FileChanged(DownloadFileChangedEventArgs e)
        {
            Pb_File.Maximum = e.TotalFileCount;
            Pb_File.Value = e.ProgressedFileCount;
            status.Text = $"{e.FileKind} : {e.FileName} ({e.ProgressedFileCount}/{e.TotalFileCount})";
            //Debug.WriteLine(Lv_Status.Text);
        }
        private async Task initializeLauncher(MinecraftPath path)
        {
            txtMCPath.Text = path.BasePath;
            gamepath = path;

            launcher = new CMLauncher(path);
            launcher.FileChanged += Launcher_FileChanged;
            launcher.ProgressChanged += Launcher_ProgressChanged;
            await refreshVersions(null);
        }

        private async Task refreshVersions(string showVersion)
        {
            cmbxVer.Items.Clear();

            var versions = await launcher.GetAllVersionsAsync();

            bool showVersionExist = false;
            foreach (var item in versions)
            {
                if (showVersion != null && item.Name == showVersion)
                    showVersionExist = true;
                cmbxVer.Items.Add(item.Name);
            }

            if (showVersion == null || !showVersionExist)
                btnSetLastVersion_Click(null, null);
            else
                cmbxVer.Text = showVersion;
        }
        private async void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (vars.session == null)
            {
                MessageBox.Show("Login First");
                var logindialog = new Login();
                logindialog.IsPrimaryButtonEnabled = false;
                logindialog.IsSecondaryButtonEnabled = false;
                logindialog.ShowAsync();
                return;
            }

            if (cmbxVer.SelectedItem == null)
            {
                MessageBox.Show("Select Version");
                return;
            }
            try
            {
                // create LaunchOption
                var launchOption = new MLaunchOption()
                {
                    MaximumRamMb = int.Parse(txtbxRam.Text),
                    Session = vars.session,

                    VersionType = "",
                    GameLauncherName = "",
                    GameLauncherVersion = "",

                    FullScreen = false,

                    ServerIp = "",
                    MinimumRamMb = 256,
                    DockName = "",
                    DockIcon = ""
                };

                if (!string.IsNullOrEmpty(javapath))
                {
                    launchOption.JavaPath = javapath;
                }
//                if (!string.IsNullOrEmpty(Txt_ServerPort.Text))
  //                  launchOption.ServerPort = int.Parse(Txt_ServerPort.Text);

          //      if (!string.IsNullOrEmpty(Txt_ScWd.Text) && !string.IsNullOrEmpty(Txt_ScHt.Text))
            //    {
              //      launchOption.ScreenHeight = int.Parse(Txt_ScHt.Text);
                //    launchOption.ScreenWidth = int.Parse(Txt_ScWd.Text);
          //      }

            //    if (!string.IsNullOrEmpty(Txt_JavaArgs.Text))
            //        launchOption.JVMArguments = Txt_JavaArgs.Text.Split(' ');

                //if (rbParallelDownload.Checked)
               // {
                    System.Net.ServicePointManager.DefaultConnectionLimit = 256;
                    launcher.FileDownloader = new AsyncParallelDownloader();
               // }
               // else
               //     launcher.FileDownloader = new SequenceDownloader();

                if (cbSkipAssetsDownload.IsChecked == true)
                    launcher.GameFileCheckers.AssetFileChecker = null;
                else if (launcher.GameFileCheckers.AssetFileChecker == null)
                    launcher.GameFileCheckers.AssetFileChecker = new AssetChecker();

                // check file hash or don't check
                if (launcher.GameFileCheckers.AssetFileChecker != null)
                    launcher.GameFileCheckers.AssetFileChecker.CheckHash = !cbSkipHashCheck.IsChecked == true;
                if (launcher.GameFileCheckers.ClientFileChecker != null)
                    launcher.GameFileCheckers.ClientFileChecker.CheckHash = !cbSkipHashCheck.IsChecked == true;
                if (launcher.GameFileCheckers.LibraryFileChecker != null)
                    launcher.GameFileCheckers.LibraryFileChecker.CheckHash = !cbSkipHashCheck.IsChecked == true;

                var process = await launcher.CreateProcessAsync(cmbxVer.Text, launchOption); // Create Arguments and Process

                // process.Start(); // Just start game, or
                StartProcess(process); // Start Process with debug options
            }
            catch (FormatException fex) // int.Parse exception
            {
                MessageBox.Show("Failed to create MLaunchOption\n\n" + fex);
            }
            catch (MDownloadFileException mex) // download exception
            {
                MessageBox.Show(
                    $"FileName : {mex.ExceptionFile.Name}\n" +
                    $"FilePath : {mex.ExceptionFile.Path}\n" +
                    $"FileUrl : {mex.ExceptionFile.Url}\n" +
                    $"FileType : {mex.ExceptionFile.Type}\n\n" +
                    mex.ToString());
            }
            catch (Win32Exception wex) // java exception
            {
                MessageBox.Show(wex + "\n\nIt seems your java setting has problem");
            }
            catch (Exception ex) // all exception
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                // re open log form
                if (logPage != null)
                    logPage.Hide();

                logPage = new GameLog();
                UI(true);
                logPage.ShowAsync();

                // enable ui
            }
        }
        private void UI(bool value)
        {
            cmbxVer.IsEnabled = value;
            btnLaunch.IsEnabled = value;
        }
        private void StartProcess(Process process)
        {
            File.WriteAllText("launcher.txt", process.StartInfo.Arguments);
            output(process.StartInfo.Arguments);

            // process options to display game log

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            output(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            output(e.Data);
        }

        void output(string msg)
        {
            GameLog.AddLog(msg);
        }

        private void btnSetLastVersion_Click(object sender, RoutedEventArgs e)
        {
            cmbxVer.Text = launcher.Versions?.LatestReleaseVersion?.Name;

        }

        private async void btnChangeMCpath_Click(object sender, RoutedEventArgs e)
        {
            var dia = new GamePathPage(gamepath);
            dia.IsPrimaryButtonEnabled = false;
            dia.IsSecondaryButtonEnabled = false;
            dia.ShowAsync();
            await initializeLauncher(dia.minecraftPath);
            dia = null;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

        }
    }
}