﻿using System;
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
        System.Windows.Threading.DispatcherTimer timer;
        CMLauncher launcher;
        readonly MSession session;
        MinecraftPath gamepath;
        string javapath;
        public static bool isGameRuns;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        GameLog logPage;
        public MainWindow()
        {
            InitializeComponent();
            var logindialog = new Login();
            logindialog.IsPrimaryButtonEnabled = false;
            logindialog.IsSecondaryButtonEnabled = false;
            logindialog.ShowAsync();
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer1_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Start();
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipText = "Click to show";
            m_notifyIcon.BalloonTipTitle = "Emarld Launcher";
            m_notifyIcon.Text = "Emarld Launcher";
            m_notifyIcon.Icon = new System.Drawing.Icon("pngwing.com.ico");
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
        }
        private void Timer1_Tick(object sender, EventArgs e)
        { 
            if(isGameRuns == true)
            {
                btnLaunch.IsEnabled = false;
            }
            else
            {
                btnLaunch.IsEnabled = true;
            }
        }

            private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var defaultpath = new MinecraftPath(MinecraftPath.GetOSDefaultPath());
            await initializeLauncher(defaultpath);
            var computerMemory = Util.GetMemoryMb();
            if (computerMemory == null)
            {
                MessageBox.Show("Failed to get computer memory");
                return;
            }

            var max = computerMemory / 3;
            if (max < 1024)
            {
                max = 1024;
            }
            else if (max > 8192)
            {
                max = 8192;
            }

            var min = max / 10;

            txtbxMaxRam.Text = max.ToString();
            txtbxMinRam.Text = min.ToString();
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
                {
                    showVersionExist = true;
                }

                cmbxVer.Items.Add(item.Name);
            }

            if (showVersion == null || !showVersionExist)
            {
                btnSetLastVersion_Click(null, null);
            }
            else
            {
                cmbxVer.Text = showVersion;
            }
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

            if (btnMCVer.Content.ToString() == "Version")
            {
                MessageBox.Show("Select Version");
                return;
            }
            try
            {
                // create LaunchOption
                var launchOption = new MLaunchOption()
                {
                    MaximumRamMb = int.Parse(txtbxMaxRam.Text),
                    Session = vars.session,

                    VersionType = "",
                    GameLauncherName = "",
                    GameLauncherVersion = "",

                    FullScreen = false,

                    ServerIp = "",
                    MinimumRamMb = int.Parse(txtbxMaxRam.Text),
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

                var process = await launcher.CreateProcessAsync(btnMCVer.Content.ToString(), launchOption); // Create Arguments and Process

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
                // re open log page
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
            isGameRuns = true;
            Hide();
            if (m_notifyIcon != null)
            {
                m_notifyIcon.ShowBalloonTip(2000);
            }

            var th = new System.Threading.Thread(() =>
            {
                process.WaitForExit();
                this.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate ()
                        {
                            ShowWindow();
                            isGameRuns = false;
                        }
                        ));
            });
            th.Start();
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
            await dia.ShowAsync();
            await initializeLauncher(dia.minecraftPath);
            dia = null;
        }

        private void VerMenuItem_ClickEx(MenuItem item)
        {
            if (item.Header.ToString() == "Latest")
            {
                btnMCVer.Content = launcher.Versions?.LatestReleaseVersion?.Name;
                return;
            }
            else if(item.Header.ToString() == "Latest Snapshot")
            {
                btnMCVer.Content = launcher.Versions?.LatestSnapshotVersion?.Name;
                return;
            }
            else
            {
                btnMCVer.Content = item.Header;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(sender is MenuItem m)
            {
                VerMenuItem_ClickEx(m);
            }
        }


        void OnClose(object sender, CancelEventArgs args)
        {
            m_notifyIcon.Dispose();
            m_notifyIcon = null;
        }

        private WindowState m_storedWindowState = WindowState.Normal;
        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }
        public void ShowWindow()
        {
            Show();
            WindowState = m_storedWindowState;
        }
        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }



    }
}