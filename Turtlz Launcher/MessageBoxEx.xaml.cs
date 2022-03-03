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
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for MessageBoxEx.xaml
    /// </summary>
    public partial class MessageBoxEx : Window
    {
        public MessageBoxResult result { get; set; }
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        public MessageBoxEx(string content, string title, MessageBoxButton btns, MessageBoxImage img)
        {
            InitializeComponent();
            this.Title = title;
            msg.Text = content;
            if (img == MessageBoxImage.Error)
            {
                icon.Glyph = "\uE783";
            }
            if (img == MessageBoxImage.Information)
            {
                icon.Glyph = "\uE946";
            }
            if (img == MessageBoxImage.Question)
            {
                icon.Glyph = "\uF142";
            }
            if (img == MessageBoxImage.None)
            {
                icon.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
