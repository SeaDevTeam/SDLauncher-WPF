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
using ModernWpf.Controls;
using CmlLib.Core;
using System.IO;
namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for GamePathPage.xaml
    /// </summary>
    public partial class GamePathPage : ContentDialog
    {
        public MinecraftPath minecraftPath;
        public GamePathPage()
        {
            InitializeComponent();
        }
        public GamePathPage(MinecraftPath path)
        {
            minecraftPath = path;
            InitializeComponent();

            if (minecraftPath == null)
                btnDefalt_Click(null, null);
            else
                apply(minecraftPath);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var mc = new MinecraftPath(txtMCpath.Text)
            {
                Runtime = minecraftPath.Runtime,
                Assets = Path.Combine(MinecraftPath.GetOSDefaultPath(), "assets")
            };
            apply(mc);
            this.Hide();
        }

        private void btnDefalt_Click(object sender, RoutedEventArgs e)
        {
            var defaultPath = MinecraftPath.GetOSDefaultPath();
            apply(new MinecraftPath(defaultPath));
        }
        void apply(MinecraftPath path)
        {
            txtMCpath.Text = path.BasePath;
            minecraftPath = path;

        }
    }
}
