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
using CmlLib.Utils;
using System.Collections.ObjectModel;

namespace Turtlz_Launcher
{
    /// <summary>
    /// Interaction logic for optionstxt.xaml
    /// </summary>
    public partial class optionstxt : ContentDialog
    {
        GameOptionsFile optionFile;
        public static string Path { get; set; }
        public optionstxt(string path)
        {
            Path = path;
            InitializeComponent();
        }

        public static ObservableCollection<LVItems> items = new ObservableCollection<LVItems>();
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            Path = txtPath.Text;

            optionFile = GameOptionsFile.ReadFile(Path);
            foreach (var item in optionFile)
            {
                items.Add(new LVItems() { key = item.Key, val = item.Value });
            }
            dataGrid.ItemsSource = items;
        }
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.Filter = "Options Text File|*.txt";
            bool? result = openFileDlg.ShowDialog();
            if (result == true)
            {
                txtPath.Text = openFileDlg.FileName;
                Path = txtPath.Text;

                optionFile = GameOptionsFile.ReadFile(Path);
                foreach (var item in optionFile)
                {
                    items.Add(new LVItems() { key = item.Key, val = item.Value });
                }
                dataGrid.ItemsSource = items;
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtPath.Text = Path;
            btnLoad_Click(null, null);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItems.Count == 0)
                return;
            dynamic selectedItem = dataGrid.SelectedItems[0];
            OpenPanel(selectedItem.key, selectedItem.val, false);
        }
        private void OpenPanel(string key, string value, bool enableKey)
        {
            pnledit.Visibility = Visibility.Visible;
            pnlTollBar.Visibility = Visibility.Collapsed;
            txtKey.Text = key;
            txtVal.Text = value;
            string oKey = key;
            string oValue = value;
            txtKey.IsEnabled = enableKey;
        }

        string oKey = "";
        string oValue = "";
        private void btnEditOK_Click(object sender, RoutedEventArgs e)
        {
            if (txtKey.IsEnabled)
                dataGrid.Items.Add(new LVItems { key = txtKey.Text, val = txtVal.Text });
            else
                foreach (LVItems item in dataGrid.ItemsSource)
                {
                    if (item.key == oKey)
                    {
                        dataGrid.Items.Remove(item);
                        dataGrid.Items.Add(new LVItems { key = txtKey.Text, val = txtVal.Text });
                        dataGrid.SelectedItem = new LVItems { key = txtKey.Text, val = txtVal.Text };
                    }
                    else
                    {
                    }
                }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            fly.Hide();
            items = (ObservableCollection<LVItems>)dataGrid.ItemsSource;
            items.Add(new LVItems() { key = "NewOptionName", val = "Value" });
            dataGrid.ItemsSource = items;
            scrollView.ScrollToEnd();
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            items = (ObservableCollection<LVItems>)dataGrid.ItemsSource;
            dynamic selectedItem = dataGrid.SelectedItems[0];
            //dataGrid.Items.Remove(dataGrid.SelectedItems[0]);
            int n = dataGrid.SelectedItems.Count;
            for (int i = 0; i < n; i++)
            {
                items.Remove((LVItems)dataGrid.SelectedItems[0]);
            }
            dataGrid.ItemsSource = items;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            fly.Hide();
            items = (ObservableCollection<LVItems>)dataGrid.ItemsSource;
            foreach (LVItems item in items)
            {
                MessageBox.Show(item.key + " : " + item.val);
                optionFile.SetRawValue(item.key, item.val);
            }

            optionFile.Save();
            this.Hide();
        }

        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (dataGrid.SelectedItems.Count > 0)
            {
                btnDel.IsEnabled = true;
            }
            else
            {
                btnDel.IsEnabled = false;
            }
        }

        private void btnEditCancel_Click(object sender, RoutedEventArgs e)
        {
            fly.Hide();
            this.Hide();
        }
    }

    public class LVItems
    {
        public string key { get; set; }

        public string val { get; set; }
    }

}
