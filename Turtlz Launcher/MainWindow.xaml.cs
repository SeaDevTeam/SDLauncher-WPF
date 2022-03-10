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
using WPFUI;
using CmlLib.Utils;
using System.IO;
using DiscordRPC;
using DiscordRPC.Logging;
using CmlLib.Core.Installer.FabricMC;
using System.IO.Compression;
using Button = DiscordRPC.Button;
using System.Windows.Interop;
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
        System.Windows.Forms.ContextMenuStrip menuStrip = new System.Windows.Forms.ContextMenuStrip();
        public static System.Windows.Threading.DispatcherTimer timer;
        public static System.Windows.Threading.DispatcherTimer discordTimer;
        CMLauncher launcher;
        readonly MSession session;
        MinecraftPath gamepath;
        string javapath;
        string launchVer;
        public static bool isGameRuns;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        GameLog logPage;
        public static string currentVer;
        int minRam;
        int VerSelectAdvaced;
        bool UIstate = true;
        Changelogs logs;

        async void LoadSettings()
        {
            status.Text = "Loading Settings";
            UI(false);
            vars.Serv = Properties.Settings.Default.MCServer;
            vars.port = Properties.Settings.Default.MCport;
            switchAutoLogin.IsOn = Properties.Settings.Default.autologin;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.MCPath))
            {

                var defaultpath = new MinecraftPath(Properties.Settings.Default.MCPath);
                try
                {
                    await initializeLauncher(defaultpath);
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(this, "Error occurred when trying to initialize the launcher, Do you want to restart the app ?" + Environment.NewLine + ex.Message + Environment.NewLine + "It says your internet could not make a launch option with the path, Can't connect to mojang servers", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    {
                        Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                        Application.Current.Shutdown();
                    }
                    Application.Current.Shutdown();
                }
            }
            else
            {
                var defaultpath = new MinecraftPath(MinecraftPath.GetOSDefaultPath());
                try
                {
                    await initializeLauncher(defaultpath);
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(this, "Error occurred when trying to initialize the launcher, Do you want to restart the app ?" + Environment.NewLine + ex.Message + Environment.NewLine + "It says your internet could not make a launch option with the path, Can't connect to mojang servers", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    {
                        Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                        Application.Current.Shutdown();
                    }
                    Application.Current.Shutdown();

                }
            }
            status.Text = "Intializing RAM";
            var computerMemory = Util.GetMemoryMb();
            if (computerMemory == null)
            {
                if (MessageBox.Show(this, "Error occurred when trying to Getting the RAM of the PC, Do you want to restart ?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                    Application.Current.Shutdown();
                }
                Application.Current.Shutdown();
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
            switchRPC.IsOn = Properties.Settings.Default.UseDiscordRPC;
            switchhide.IsOn = Properties.Settings.Default.Autohide;
            txtbxStats.Text = Properties.Settings.Default.RPCStats;
            switchgamelog.IsOn = Properties.Settings.Default.ShowGameLog;
            cbSkipAssetsDownload.IsChecked = Properties.Settings.Default.SkipAssetsDown;
            launchVer = Properties.Settings.Default.StringVer;
            cbSkipHashCheck.IsChecked = Properties.Settings.Default.SkipHashCheck;
            swtchVer.IsChecked = Properties.Settings.Default.UseAdvacedVer;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.CurrentVer))
            {
                btnMCVer.Content = Properties.Settings.Default.CurrentVer;
                cmbxVer.Text = Properties.Settings.Default.CurrentVer;
            }
            status.Text = "Ready";
            UI(true);
            LoadLogs();
        }
        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            menuStrip.Items.Add("Open", null, (s, e) => this.Show());
            menuStrip.Items.Add("Kill Minecraft", null, (s, e) => btnMCKill_Click(null, null));
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer1_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Start();
            discordTimer = new System.Windows.Threading.DispatcherTimer();
            discordTimer.Tick += RPCTick;
            discordTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            discordTimer.Start();
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipTitle = "Emarld Launcher";
            m_notifyIcon.BalloonTipText = "Right click on the context menu of tray icon to restore";
            m_notifyIcon.Text = "Emarld Launcher";
            m_notifyIcon.ContextMenuStrip = menuStrip;
            m_notifyIcon.Icon = new System.Drawing.Icon("app.ico");
        }

        async void LoadLogs()
        {
            status.Text = "Loading Changelogs";
            try
            {
                logs = await Changelogs.GetChangelogs();
                if (launcher.Versions.LatestSnapshotVersion.Name != launcher.Versions.LatestReleaseVersion.Name)
                {
                    LoadLogsnext(await logs.GetChangelogHtml(launcher.Versions.LatestSnapshotVersion.Name), launcher.Versions.LatestSnapshotVersion.Name);
                }
                LoadLogsnext(await logs.GetChangelogHtml("1.18.2"), "1.18.2");
                LoadLogsnext(await logs.GetChangelogHtml("1.18.1"), "1.18.1");
                LoadLogsnext(await logs.GetChangelogHtml("1.18"), "1.18");
                LoadLogsnext(await logs.GetChangelogHtml("1.17.1"), "1.7.1");
                LoadLogsnext(await logs.GetChangelogHtml("1.16"), "1.16");
                clearclipBoard();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Getting changelogs failed " + Environment.NewLine + ex.Message);
            }
            status.Text = "Ready";
        }
        void clearclipBoard()
        {
            System.Windows.Forms.WebBrowser wb = new System.Windows.Forms.WebBrowser();
            wb.Navigate("about:blank");
            wb.Document.Write("<p>The Changelogs was on your clipboard</p>");
            wb.Document.ExecCommand("SelectAll", false, null);
            wb.Document.ExecCommand("Copy", false, null);
            wb.Dispose();

        }
        void LoadLogsnext(string body, string ver)
        {
            status.Text = "Fletching: v" + ver;
            System.Windows.Forms.WebBrowser wb = new System.Windows.Forms.WebBrowser();
            wb.Navigate("about:blank");
            var fullbody = "<style>" + Environment.NewLine + "p,h1,li,span,body,html {" + Environment.NewLine + "font-family:\"Segoe UI\";" + Environment.NewLine + "}" + Environment.NewLine + "</style>" + Environment.NewLine + "<body>" + "<h1>Version " + ver + "</h1>" + body + "</body>";
            wb.Document.Write(fullbody.Replace("h1", "h2").ToString());
            wb.Document.ExecCommand("SelectAll", false, null);
            wb.Document.ExecCommand("Copy", false, null);
            richtxt.Paste();
            wb.Dispose();
            TextRange allText = new TextRange(richtxt.Document.ContentStart, richtxt.Document.ContentEnd);
            allText.ApplyPropertyValue(RichTextBox.FontFamilyProperty, new FontFamily("Segoe UI"));
            allText.ApplyPropertyValue(RichTextBox.ForegroundProperty, status.Foreground);
            status.Text = "Ready";
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(vars.Serv))
            {
                if (vars.port != null)
                {
                    if (vars.port == 25565)
                    {
                        join2serv.Content = vars.Serv;
                    }
                    else
                    {
                        join2serv.Content = vars.Serv + ":" + vars.port;
                    }
                }
            }
            else
            {
                join2serv.Content = "Servers";
            }
            lblRam.Text = "Ram: " + ((int)sliderRAM.Value).ToString() + " MB";
            if (vars.session != null)
            {
                try
                {
                    lblWelcome.Text = "Weclome! " + vars.UserName;
                    prfPic.DisplayName = vars.UserName;
                    prpFly.DisplayName = vars.UserName;
                    btnLogin.Content = "Change Account";
                    lbluserFly.Text = vars.UserName;
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                lblWelcome.Text = "Weclome!";
                prfPic.DisplayName = "";
                prpFly.DisplayName = "";
                lbluserFly.Text = "Login";
                btnLogin.Content = "Login";

            }
            if (swtchVer.IsChecked == true)
            {
                cmbxVer.Visibility = Visibility.Visible;
                VerSelectAdvaced = 1;
                btnMCVer.Visibility = Visibility.Collapsed;
            }
            else
            {
                VerSelectAdvaced = 0;
                cmbxVer.Visibility = Visibility.Collapsed;
                btnMCVer.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Dark;
            if (Properties.Settings.Default.autologin)
            {
                if (Properties.Settings.Default.session != null)
                {
                    vars.session = Properties.Settings.Default.session;
                    vars.UserName = Properties.Settings.Default.session.Username;
                }
                else
                {
                    var logindialog = new Login();
                    logindialog.IsPrimaryButtonEnabled = false;
                    logindialog.IsSecondaryButtonEnabled = false;
                    logindialog.ShowAsync();
                }
            }
            else
            {
                var logindialog = new Login();
                logindialog.IsPrimaryButtonEnabled = false;
                logindialog.IsSecondaryButtonEnabled = false;
                logindialog.ShowAsync();
            }
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
            UI(false);
            status.Text = "Initializing...";
            txtMCPath.Text = path.BasePath;
            gamepath = path;

            launcher = new CMLauncher(path);
            launcher.FileChanged += Launcher_FileChanged;
            launcher.ProgressChanged += Launcher_ProgressChanged;
            await refreshVersions(null);
            UI(true);
        }

        public static CmlLib.Core.Version.MVersionCollection mcVers;
        public static CmlLib.Core.Version.MVersionCollection mcFabricVers;
        private async Task refreshVersions(string showVersion)
        {
            var lastUIsts = UIstate;
            UI(false);
            status.Text = "Getting Available Versions";
            cmbxVer.Items.Clear();
            btnMCVer.Content = "Version";
            launchVer = "";
            mcVers = await launcher.GetAllVersionsAsync();

            mcFabricVers = await new FabricVersionLoader().GetVersionMetadatasAsync();

            bool showVersionExist = false;
            foreach (var item in mcFabricVers)
            {
                cmbxVer.Items.Add(item.Name);
            }
            foreach (var item in mcVers)
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
            status.Text = "Ready";
            if (lastUIsts)
            {
                UI(true);
            }
        }
        string MCver = "";
        private async void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            UI(false);
            if (vars.session == null)
            {
                MessageBox.Show(this, "Please login");
                var logindialog = new Login();
                logindialog.IsPrimaryButtonEnabled = false;
                logindialog.IsSecondaryButtonEnabled = false;
                logindialog.ShowAsync();
                UI(true);
                return;
            }
            if (VerSelectAdvaced == 0)
            {
                if (btnMCVer.Content.ToString() == "Version")
                {
                    MessageBox.Show(this, "Select a version");
                    UI(true);
                    return;
                }
                else
                {
                    MCver = launchVer;
                }
            }
            else if (VerSelectAdvaced == 1)
            {
                if (string.IsNullOrEmpty(cmbxVer.Text.ToString()))
                {
                    MessageBox.Show(this, "Select a version");
                    UI(true);
                    return;
                }
                else
                {
                    MCver = cmbxVer.Text.ToString();
                }

            }
            string ip = "";

            if (!string.IsNullOrEmpty(vars.Serv))
            {
                ip = vars.Serv;
            }
            isGameRuns = true;
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

                    ServerIp = ip,
                    MinimumRamMb = minRam,
                    DockName = "",
                    DockIcon = ""
                };

                if (!string.IsNullOrEmpty(javapath))
                {
                    launchOption.JavaPath = javapath;
                }
                if (vars.port != null && vars.port != 25565)
                {
                    launchOption.ServerPort = (int)vars.port;
                }
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
                MessageBox.Show(this, "Failed to create MLaunchOption\n\n" + fex);
            }
            catch (MDownloadFileException mex) // download exception
            {
                MessageBox.Show(this, 
                    $"FileName : {mex.ExceptionFile.Name}\n" +
                    $"FilePath : {mex.ExceptionFile.Path}\n" +
                    $"FileUrl : {mex.ExceptionFile.Url}\n" +
                    $"FileType : {mex.ExceptionFile.Type}\n\n" +
                    mex.ToString());
            }
            catch (Win32Exception wex) // java exception
            {
                MessageBox.Show(this, wex + "\n\nIt seems your java setting has problem");
            }
            catch (Exception ex) // all exception
            {
                MessageBox.Show(this, ex.ToString());
            }
            finally
            {
                logPage = new GameLog();
                if (switchgamelog.IsOn)
                {
                    // re open log page
                    logPage = new GameLog();
                    logPage.CloseButtonText = "Close";
                    logPage.ShowAsync();
                }

                // enable ui
            }
        }
        private void UI(bool value)
        {
            UIstate = value;
            btnLogin.IsEnabled = value;
            pnlLaunch.IsEnabled = value;
            sliderRAM.IsEnabled = value;
            swtchVer.IsEnabled = value;
            btnOpt.IsEnabled = value;
            btnRefresh.IsEnabled = value;
            swtchVer.IsEnabled = value;
            btnChangeMCpath.IsEnabled = value;
            join2serv.IsEnabled = value;
            cbSkipAssetsDownload.IsEnabled = value;
            cbSkipHashCheck.IsEnabled = value;
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
            if (switchhide.IsOn)
            {
                Hide();
            }
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
                            UI(true);
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

        private async void VerMenuItem_ClickEx(MenuItem item)
        {
            if (item.Header.ToString() == "Latest")
            {
                btnMCVer.Content = launcher.Versions?.LatestReleaseVersion?.Name;
                launchVer = btnMCVer.Content.ToString();
                return;
            }
            else if (item.Header.ToString() == "Latest Snapshot")
            {
                btnMCVer.Content = launcher.Versions?.LatestSnapshotVersion?.Name;
                launchVer = btnMCVer.Content.ToString();
                return;
            }
            else if (item.Header.ToString() == "OptiFine 1.18.2")
            {
                CheckOptiFine("1.18.2", "1.18.2-OptiFine_HD_U_H6_pre1", item);
            }
            else if (item.Header.ToString() == "OptiFine 1.18.1")
            {
                CheckOptiFine("1.18.1", "1.18.1-OptiFine_HD_U_H4", item);
            }
            else if (item.Header.ToString() == "OptiFine 1.17.1")
            {
                CheckOptiFine("1.17.1", "1.17.1-OptiFine_HD_U_H1", item);
            }
            else if (item.Header.ToString() == "OptiFine 1.16.5")
            {
                CheckOptiFine("1.16.5", "OptiFine 1.16.5", item);
            }
            else if (item.Header.ToString() == "Fabric 1.18.1")
            {
                CheckFabric("1.18.1", "fabric-loader-0.13.3-1.18.1", item);
            }
            else if (item.Header.ToString() == "Fabric 1.17.1")
            {
                CheckFabric("1.17.1", "fabric-loader-0.13.3-1.17.1", item);
            }
            else if (item.Header.ToString() == "Fabric 1.16.5")
            {
                CheckFabric("1.16.5", "fabric-loader-0.13.3-1.16.5", item);
            }
            else
            {
                btnMCVer.Content = item.Header;
                launchVer = btnMCVer.Content.ToString();
            }
        }

        void CheckFabric(string mcver,string modver, MenuItem mit)
        {
            bool exists = false;
            foreach (var veritem in mcFabricVers)
            {
                if (veritem.Name == modver)
                {
                    exists = true;
                }
            }
            if (exists)
            {
                launchVer = modver;
                btnMCVer.Content = mit.Header.ToString();
                status.Text = "Getting Fabric";
                UI(false);
                System.Threading.Thread thread = new System.Threading.Thread(async () =>
                {
                    var fabric = mcFabricVers.GetVersionMetadata(launchVer);
                    await fabric.SaveAsync(gamepath);
                    this.Dispatcher.Invoke(
                            System.Windows.Threading.DispatcherPriority.Normal,
                            new Action(
                            async delegate ()
                            {
                                UI(true);
                                status.Text = "Ready";
                                await refreshVersions(null);
                                launchVer = modver;
                                btnMCVer.Content = mit.Header.ToString();
                            }
                            ));
                });
                thread.Start();
                status.Text = "Ready";
            }
            else
            {
                if (MessageBox.Show(this, "To run " + mit.Header.ToString() +" you need to have installed version " + mcver + ". Vanilla,Do you want to install now ?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    btnMCVer.Content = mcver;
                    launchVer = mcver;
                }
            }
        }

        void CheckOptiFine(string mcver, string modVer, MenuItem mit)
        {
            bool exists = false;
            foreach (var veritem in mcVers)
            {
                if (veritem.Name == modVer)
                {
                    exists = true;
                }
            }
            if (exists)
            {
                btnMCVer.Content = mit.Header.ToString();
                launchVer = modVer;
            }
            else
            {
                if (MessageBox.Show(this, "Couldn't find OptiFine installed on this minecraft. Do you want to download and install from our servers ?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (File.Exists(gamepath.BasePath + "\\versions\\" + mcver + "\\" + mcver + ".jar"))
                    {
                        if (Directory.Exists(gamepath.BasePath + "\\libraries\\optifine"))
                        {
                            if (mcver == "1.18.2")
                            {
                                btnMCVer.Content = mit.Header.ToString();
                                launchVer = modVer;
                                isOptiFineRuns = true;
                                UI(false);
                                optver = ": " + mcver;
                                OptFineDownload("https://raw.githubusercontent.com/Chaniru22/SDLauncher/main/OptiFine-1.18.2.zip", Directory.GetCurrentDirectory() + "\\OptiFine-" + mcver + ".zip", ModType.ver);
                            }
                            if (mcver == "1.18.1")
                            {
                                btnMCVer.Content = mit.Header.ToString();
                                launchVer = modVer;
                                isOptiFineRuns = true;
                                UI(false);
                                optver = ": " + mcver;
                                OptFineDownload("https://raw.githubusercontent.com/Chaniru22/SDLauncher/main/OptiFine-1.18.1.zip", Directory.GetCurrentDirectory() + "\\OptiFine-" + mcver + ".zip", ModType.ver);
                            }
                            else if (mcver == "1.17.1")
                            {
                                btnMCVer.Content = mit.Header.ToString();
                                launchVer = modVer;
                                isOptiFineRuns = true;
                                UI(false);
                                optver = ": " + mcver;
                                OptFineDownload("https://raw.githubusercontent.com/Chaniru22/SDLauncher/main/OptiFine-1.17.1.zip", Directory.GetCurrentDirectory() + "\\OptiFine-" + mcver + ".zip", ModType.ver);
                            }
                            else if (mcver == "1.16.5")
                            {
                                btnMCVer.Content = mit.Header.ToString();
                                launchVer = modVer;
                                isOptiFineRuns = true;
                                UI(false);
                                optver = ": " + mcver;
                                OptFineDownload("https://raw.githubusercontent.com/Chaniru22/SDLauncher/main/OptiFine-1.16.5.zip", Directory.GetCurrentDirectory() + "\\OptiFine-" + mcver + ".zip", ModType.ver);
                            }
                        }
                        else
                        {
                            isOptiFineRuns = true;
                            MessageBox.Show(this, "This will download main OptiFine library, Please click again " + mit.Header.ToString() + " (after download and extract the main OptiFine) to install optifine of that version !");
                            optver = " Lib";
                            OptFineDownload("https://raw.githubusercontent.com/Chaniru22/SDLauncher/main/optifine.zip", Directory.GetCurrentDirectory() + "\\OptiFine.zip", ModType.lib); 
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "You have to install & run minecraft version " + mcver + " one time to install OptiFine", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        btnMCVer.Content = mcver;
                        launchVer = mcver;
                    }
                }
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
                MCver = btnMCVer.Content.ToString();
            }
            else if (VerSelectAdvaced == 1)
            {
                bools = true;
                MCver = cmbxVer.Text.ToString();
            }
            //MCPath / RAM / DRPC / MCVer
            Properties.Settings.Default.MCPath = gamepath.BasePath;
            Properties.Settings.Default.CurrentRam = (int)sliderRAM.Value;
            Properties.Settings.Default.UseDiscordRPC = switchRPC.IsOn;
            Properties.Settings.Default.Autohide = switchhide.IsOn;
            Properties.Settings.Default.ShowGameLog = switchgamelog.IsOn;
            Properties.Settings.Default.RPCStats = txtbxStats.Text;
            Properties.Settings.Default.MCServer = vars.Serv;
            Properties.Settings.Default.MCport = vars.port;
            Properties.Settings.Default.CurrentVer = MCver;
            Properties.Settings.Default.StringVer = launchVer;
            if (vars.session != null)
            {
                Properties.Settings.Default.session = vars.session;
            }
            Properties.Settings.Default.autologin = switchAutoLogin.IsOn;
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
                var result = MessageBox.Show(this, "Minecraft version:" + currentVer + " is running/launching, Do you really want to close?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
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
            else if (isOptiFineRuns)
            {
                var result = MessageBox.Show(this, "OptiFine is installing, Do you really want to close?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Information);
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
                m_notifyIcon.Dispose();
                m_notifyIcon = null;
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
            scrlSettings.ScrollToTop();
            Loginflyout.Hide();
            SettingsPane.IsPaneOpen = !SettingsPane.IsPaneOpen;
        }

        private void TitleBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            Loginflyout.Hide();
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
                SmallImageKey = "appico",
                SmallImageText = "SDLauncher"
            },
            Buttons = new Button[]
                {
                    new Button() { Label = "Get SDLauncher", Url = "https://github.com/Chaniru22/SDLauncher" }
                }
        };
        void RPCInitialize()
        {
            try
            {
                RPCclient.Dispose();
            }
            catch
            {

            }
            presence.Timestamps = new Timestamps(starttime, null);
            RPCclient = null;
            RPCclient = new DiscordRpcClient("945758066161356932");
            RPCclient.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            RPCclient.OnReady += (sender, e) =>
            {
                Debug.WriteLine("RPC Received Ready from user " + e.User.Username);
            };

            RPCclient.OnPresenceUpdate += (sender, e) =>
            {
                Debug.WriteLine("RPC Received Update!");
            };
            
            try
            {
                RPCclient.Initialize();
                RPCclient.SetPresence(presence);
                RPCclient.UpdateStartTime(starttime);
                isRPCon = true;
            }
            catch
            {
                isRPCon = false;
            }

        }
        DateTime starttime = DateTime.UtcNow;
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
                    MCver = " version " + btnMCVer.Content.ToString();
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
                    MCver = " version " + cmbxVer.Text.ToString();
                }

            }
            if (switchRPC.IsOn)
            {
                if (!isRPCon)
                {
                    isRPCon = true;
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
                    Details = "Going to play",
                    State = txtbxStats.Text,
                    Assets = new Assets()
                    {
                        LargeImageKey = "minecraft",
                        LargeImageText = "Minecraft" + MCver,
                        SmallImageKey = "appico",
                        SmallImageText = "SDLauncher"
                    }
            };
                tempPrec.Buttons = presence.Buttons;
                presence = tempPrec;
                presence.Timestamps = new Timestamps(starttime, null);
                if (RPCclient != null)
                {
                    if (!RPCclient.IsDisposed)
                    {
                        try
                        {
                            RPCclient.SetPresence(presence);
                            RPCclient.Invoke();
                            RPCclient.UpdateStartTime(starttime);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            if (isGameRuns)
            {
                tempPrec = new RichPresence()
                {
                    Details = "Playing as " + vars.session.Username,
                    State = txtbxStats.Text,
                    Assets = new Assets()
                    {
                        LargeImageKey = "minecraft",
                        LargeImageText = "Minecraft" + MCver,
                        SmallImageKey = "appico",
                        SmallImageText = "SDLauncher"
                    }
                   
                };
                tempPrec.Buttons = presence.Buttons;
                presence = tempPrec;
                presence.Timestamps = new Timestamps(starttime, null);
                if (RPCclient != null)
                {
                    if (!RPCclient.IsDisposed)
                    {
                        try
                        {
                            RPCclient.SetPresence(presence);
                            RPCclient.Invoke();
                            RPCclient.UpdateStartTime(starttime);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }

        private void btnOpt_Click(object sender, RoutedEventArgs e)
        {
            SettingsPane.IsPaneOpen = false;
            var path = System.IO.Path.Combine(gamepath.BasePath, "options.txt");
            var f = new optionstxt(path);
            f.IsPrimaryButtonEnabled = false;
            f.ShowAsync();
        }

        private void join2serv_Click(object sender, RoutedEventArgs e)
        {
            new openToServ().ShowAsync();
        }

        private void btnMCKill_Click(object sender, RoutedEventArgs e)
        {
            var d = new MessageBoxEx("Hello", "Title", MessageBoxButton.OK, MessageBoxImage.Error);
            d.Owner = Application.Current.MainWindow;
            //d.ShowDialog();
            try
            {
                MCprocess.Kill();
            }
            catch (Exception ex)
            {
                _ = System.Windows.Forms.MessageBox.Show("Failed to kill process (No process found)" + Environment.NewLine + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _ = refreshVersions(null);
        }
        enum ModType
        {
            lib,
            ver
        }
        string optDir;
        bool isOptiFineRuns;
        private void OptFineDownload(string link, string dir, ModType m)
        {
            optDir = dir;
            dwnOptiType = m;
            UI(false);
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(link), dir);
            });
            thread.Start();
        }

        string optver;
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
                        status.Text = "Downloading: OptiFine" + optver;
                        Pb_Progress.Maximum = 100;
                        Pb_Progress.Value = int.Parse(Math.Truncate(percentage).ToString());
                    }));
        }
        ModType dwnOptiType;
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               new Action(
               delegate ()
               {
                   status.Text = "Extracting";
               }
               ));
            if (dwnOptiType == ModType.lib)
            {
                ZipFile.ExtractToDirectory(optDir, gamepath.BasePath + @"\libraries", true);
            }
            else if (dwnOptiType == ModType.ver)
            {

                ZipFile.ExtractToDirectory(optDir, gamepath.BasePath + @"\versions", true);
            }
            this.Dispatcher.Invoke(
               System.Windows.Threading.DispatcherPriority.Normal,
               new Action(
               async delegate ()
               {
                   var oldDisver = btnMCVer.Content.ToString();
                   var oldVer = launchVer;
                   await refreshVersions(null);
                   launchVer = oldVer;
                   btnMCVer.Content = oldDisver;
                   status.Text = "Ready";
                   isOptiFineRuns = false;
                   UI(true);
               }
             ));
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            SettingsPane.IsPaneOpen = false;
            var page = new aboutPage();
            page.ShowAsync();
        }

        Chat chatdia = null;
        private void btnChat_Click(object sender, RoutedEventArgs e)
        {
            Loginflyout.Hide();
            if(chatdia != null)
            {
                chatdia.ShowAsync();
            }
            else
            {
                chatdia = new Chat();
                chatdia.ShowAsync();
            }
        }
    }
}