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
using WPFUI;
using CmlLib.Utils;
using System.IO;
using DiscordRPC;
using DiscordRPC.Logging;
//using Discord;
//using Discord.Net;
//using Discord.Webhook;
//using Discord.Rest;
// Discord.WebSocket;
// Discord.Audio;
//using Discord.API;
//using System.Web;
//using Discord.Commands;

namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static System.Windows.Threading.DispatcherTimer timer;
        public static System.Windows.Threading.DispatcherTimer discordTimer;
        CMLauncher launcher;
        readonly MSession session;
        MinecraftPath gamepath;
        string javapath;
        public static bool isGameRuns;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        GameLog logPage;
        public static string currentVer;
        int minRam;
        int VerSelectAdvaced;
        string DisToken;
        Changelogs logs;

        async void LoadSettings()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.MCPath))
            {

                var defaultpath = new MinecraftPath(Properties.Settings.Default.MCPath);
                await initializeLauncher(defaultpath);
            }
            else
            {
                var defaultpath = new MinecraftPath(MinecraftPath.GetOSDefaultPath());
                await initializeLauncher(defaultpath);
            }
            UI(false);
            status.Text = "Intializing RAM";
            var computerMemory = Util.GetMemoryMb();
            if (computerMemory == null)
            {
                MessageBox.Show("Failed to get computer memory");
                this.Close();
                return;
            }

            var max = computerMemory / 2.5;
            if (max < 1024)
            {
                max = 1024;
            }
            else if (max > 4096 && max < 4500)
            {
                max = 4096;
            }
            else if (max > 8192)
            {
                max = 8192;
            }
            var min = max / 10;
            minRam = (int)min;
            sliderRAM.Minimum = (long)(max / 7);
            sliderRAM.Maximum = (long)max;
            txtMaxRam.Text = ((int)max).ToString() + " MB";
            txtMinRam.Text = ((int)(max / 7)).ToString() + " MB";

            if (Properties.Settings.Default.CurrentRam >= min && Properties.Settings.Default.CurrentRam <= max)
            {
                sliderRAM.Value = Properties.Settings.Default.CurrentRam;
            }
            else
            {
                sliderRAM.Value = (long)(max / 2);
            }
            status.Text = "Ready";
            UI(true);
        }
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
            discordTimer = new System.Windows.Threading.DispatcherTimer();
            discordTimer.Tick += RPCTick;
            discordTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            discordTimer.Start();
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipText = "Click to show";
            m_notifyIcon.BalloonTipTitle = "Emarld Launcher";
            m_notifyIcon.Text = "Emarld Launcher";
            m_notifyIcon.Icon = new System.Drawing.Icon("pngwing.com.ico");
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
        }

        async void LoadLogs()
        {
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            lblRam.Text = "Ram: " + ((int)sliderRAM.Value).ToString() + " MB";
            if (isGameRuns == true)
            {
                btnLaunch.IsEnabled = false;
            }
            else
            {
                btnLaunch.IsEnabled = true;
            }
            if (vars.session != null)
            {
                try
                {
                    lblWelcome.Text = "Weclome! " + vars.UserName;
                    prfPic.DisplayName = vars.UserName;
                    txtlogin.Text = vars.UserName;
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                lblWelcome.Text = "Weclome!";
                prfPic.DisplayName = "";
                txtlogin.Text = "Login";

            }
            if (swtchVer.IsChecked == true)
            {
                pnlAdVer.Visibility = Visibility.Visible;
                VerSelectAdvaced = 1;
                btnMCVer.Visibility = Visibility.Collapsed;
            }
            else
            {
                VerSelectAdvaced = 0;
                pnlAdVer.Visibility = Visibility.Collapsed;
                btnMCVer.Visibility = Visibility.Visible;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Dark;
            LoadSettings();
            LoadLogs();
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
        string MCver = "";
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
            if (VerSelectAdvaced == 0)
            {
                if (btnMCVer.Content.ToString() == "Version")
                {
                    MessageBox.Show("Select Version");
                    return;
                }
                else
                {
                    MCver = btnMCVer.Content.ToString();
                }
            }
            else if (VerSelectAdvaced == 1)
            {
                if (string.IsNullOrEmpty(cmbxVer.Text.ToString()))
                {
                    MessageBox.Show("Select Version");
                    return;
                }
                else
                {
                    MCver = cmbxVer.Text.ToString();
                }

            }
            UI(false);
            try
            {
                // create LaunchOption
                var launchOption = new MLaunchOption()
                {
                    MaximumRamMb = (int)sliderRAM.Value,
                    Session = vars.session,

                    VersionType = "",
                    GameLauncherName = "",
                    GameLauncherVersion = "",

                    FullScreen = false,

                    ServerIp = "",
                    MinimumRamMb = minRam,
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
                {
                    launcher.GameFileCheckers.AssetFileChecker = null;


                if (launcher.GameFileCheckers.AssetFileChecker == null)
                        launcher.GameFileCheckers.AssetFileChecker = new AssetChecker();
                }
                // check file hash or don't check
                if (launcher.GameFileCheckers.AssetFileChecker != null)
                    launcher.GameFileCheckers.AssetFileChecker.CheckHash = !cbSkipHashCheck.IsChecked == true;
                if (launcher.GameFileCheckers.ClientFileChecker != null)
                    launcher.GameFileCheckers.ClientFileChecker.CheckHash = !cbSkipHashCheck.IsChecked == true;
                if (launcher.GameFileCheckers.LibraryFileChecker != null)
                    launcher.GameFileCheckers.LibraryFileChecker.CheckHash = !cbSkipHashCheck.IsChecked == true;
                currentVer = MCver;
                var process = await launcher.CreateProcessAsync(MCver.ToString(), launchOption); // Create Arguments and Process

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
            btnMCVer.IsEnabled = value;
            btnLaunch.IsEnabled = value;
        }
        static Process MCprocess;
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
            MCprocess = process;
            MCprocess.Start();
            MCprocess.BeginErrorReadLine();
            MCprocess.BeginOutputReadLine();
            isGameRuns = true;
            Hide();
            if (m_notifyIcon != null)
            {
                m_notifyIcon.ShowBalloonTip(2000);
            }
            var th = new System.Threading.Thread(() =>
            {
                MCprocess.WaitForExit();
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
        async void apply(MinecraftPath path)
        {
            txtMCPath.Text = path.BasePath;
            gamepath = path;
            await initializeLauncher(path);

        }

        private async void btnChangeMCpath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderdia = new System.Windows.Forms.FolderBrowserDialog();
            folderdia.SelectedPath = gamepath.ToString();
            if (folderdia.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var mc = new MinecraftPath(folderdia.SelectedPath.ToString())
                {
                    Runtime = gamepath.Runtime,
                    Assets = System.IO.Path.Combine(MinecraftPath.GetOSDefaultPath(), "assets")
                };
                apply(mc);
            }
        }

        private void VerMenuItem_ClickEx(MenuItem item)
        {
            if (item.Header.ToString() == "Latest")
            {
                btnMCVer.Content = launcher.Versions?.LatestReleaseVersion?.Name;
                return;
            }
            else if (item.Header.ToString() == "Latest Snapshot")
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
            if (sender is MenuItem m)
            {
                VerMenuItem_ClickEx(m);
            }
        }

        void SaveSettings()
        {
            bool bools = false;
            if (VerSelectAdvaced == 0)
            {
                bools = false;
            }
            else if (VerSelectAdvaced == 1)
            {
                bools = true;
            }
            //MCPath / RAM / DRPC / MCVer
            Properties.Settings.Default.MCPath = gamepath.BasePath;
            Properties.Settings.Default.CurrentRam = (int)sliderRAM.Value;
            Properties.Settings.Default.UseDiscordRPC = switchRPC.IsOn;
            Properties.Settings.Default.CurrentVer = MCver;
            Properties.Settings.Default.UseAdvacedVer = bools;
            if (cbSkipAssetsDownload.IsChecked == true)
            {
                bools = true;
            }
            else if (cbSkipAssetsDownload.IsChecked == false)
            {
                bools = false;
            }
            Properties.Settings.Default.SkipAssetsDown = bools;
            if (cbSkipHashCheck.IsChecked == true)
            {
                bools = true;
            }
            else if (cbSkipHashCheck.IsChecked == false)
            {
                bools = false;
            }
            Properties.Settings.Default.SkipHashCheck = bools;
            Properties.Settings.Default.Save();
        }
        void OnClose(object sender, CancelEventArgs args)
        {
            if (isGameRuns)
            {
                var result = MessageBox.Show("Minecraft version:" + currentVer + " is running/launching, Do you really want to close?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    m_notifyIcon.Dispose();
                    m_notifyIcon = null;
                    SaveSettings();
                }
                else
                {
                    args.Cancel = true;
                }
            }
            else
            {
                SaveSettings();
            }
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

        private void TitleBarButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPane.IsPaneOpen = !SettingsPane.IsPaneOpen;
        }

        private void TitleBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            var logindialog = new Login();
            logindialog.IsPrimaryButtonEnabled = false;
            logindialog.IsSecondaryButtonEnabled = false;
            logindialog.ShowAsync();
        }

        public DiscordRpcClient RPCclient;
        public RichPresence presence = new RichPresence()
        {
            Assets = new Assets()
            {
                LargeImageKey = "minecraft",
                LargeImageText = "Playing Minecraft",
                SmallImageKey = "nobacklarge",
                SmallImageText = "Emerald launcher"
            }
        };
        void RPCInitialize()
        {
            RPCclient = new DiscordRpcClient("943158277678714900");
            RPCclient.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            RPCclient.OnReady += (sender, e) =>
            {
                MessageBox.Show("Received Ready from user "+ e.User.Username);
            };

            RPCclient.OnPresenceUpdate += (sender, e) =>
            {
                MessageBox.Show("Received Update!");
            };

            RPCclient.Initialize();
            RPCclient.SetPresence(presence);
            isRPCon = true;
        }

        bool isRPCon;
        static string RPCstate;
        private void RPCTick(object sender, EventArgs e)
        {
            RichPresence tempPrec;
            string MCver = "";
            if (VerSelectAdvaced == 0)
            {
                if (btnMCVer.Content.ToString() == "Version")
                {
                    MCver = "";
                }
                else
                {
                    MCver = btnMCVer.Content.ToString();
                }
            }
            else if (VerSelectAdvaced == 1)
            {
                if (string.IsNullOrEmpty(cmbxVer.Text.ToString()))
                {
                    MCver = "";
                }
                else
                {
                    MCver = cmbxVer.Text.ToString();
                }

            }
            if (switchRPC.IsOn)
            {
                if (!isRPCon)
                {
                    RPCInitialize();
                }
            }
            else
            {
                if (isRPCon)
                {
                    isRPCon = false;
                    RPCclient.Dispose();
                }
            }
            if (!isGameRuns)
            {
                tempPrec = new RichPresence()
                {
                    Details = MCver,
                    State = "Going to start",
                    Assets = new Assets()
                    {
                        LargeImageKey = "nobacklarge",
                        LargeImageText = "Emerald launcher",
                    }
                };
                presence = tempPrec;
                if (RPCclient != null)
                {
                    if (!RPCclient.IsDisposed)
                    {
                        try
                        {
                            RPCclient.SetPresence(presence);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            if (!isGameRuns)
            {
                tempPrec = new RichPresence()
                {
                    Details = MCver,
                    State = "Playing",
                    Assets = new Assets()
                    {
                        LargeImageKey = "nobacklarge",
                        LargeImageText = "Emerald launcher",
                    }
                };
                presence = tempPrec;
                if (RPCclient != null)
                {
                    if (!RPCclient.IsDisposed)
                    {
                        try
                        {
                            RPCclient.SetPresence(presence);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }
    }
}