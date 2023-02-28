using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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
using TigerL10N.Service;

namespace TigerL10N.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(AppConfigService.Settings?.LastOpenFile))
                txtSrcFile.Text = AppConfigService.Settings.LastOpenFile;
            if (!string.IsNullOrEmpty(AppConfigService.Settings?.LastOpenFolder))
                txtSrcFolder.Text = AppConfigService.Settings.LastOpenFolder;
            if (!string.IsNullOrEmpty(AppConfigService.Settings?.LastSaveFile))
                txtTarFile.Text = AppConfigService.Settings.LastSaveFile;

            //projectTree.SetItem();
            //sourceTree.SetItem();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog od = new();
            var res = od.ShowDialog();
            if(res== true)
            {
                if (AppConfigService.Settings != null)
                {
                    AppConfigService.Settings.LastOpenFile =
                        txtSrcFile.Text = od.FileName;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog od = new();
            var  res = od.ShowDialog();
            if (res == true)
            {
                AppConfigService.Settings.LastSaveFile = 
                txtTarFile.Text = od.FileName;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog od = new();
            var res = od.ShowDialog();
            if (res == true)
            {
                AppConfigService.Settings.LastOpenFolder =
                txtSrcFolder.Text = od.SelectedPath.ToString();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            bool bCreateNewResource = false;
            bool bFolderConversion = false;
            string rsc_filename = txtTarFile.Text;
            string src_filename = txtSrcFile.Text;
            string src_foldername = txtSrcFolder.Text;
            
            if (!File.Exists(src_filename))
            {
                bFolderConversion = true;
            }

            if (bFolderConversion && !Directory.Exists(src_foldername))
            {
                MessageBox.Show("바꿀 대상이 없습니다.");
                return;
            }

            if (!File.Exists(rsc_filename))
            {
                MessageBox.Show("새 파일에 결과를 저장합니다");
                bCreateNewResource = true;
            }

            StringParseService.CreateParser()
                .SetReplaceSource(chkReplace.IsChecked == true)
                .SetReadResourceFile(chkNoread.IsChecked == true)
                .SetSourceFile(src_filename)
                .SetFolder(src_foldername)
                .SetResourceFile(rsc_filename)
                .SetFolderConversion(bFolderConversion)
                .SetCreateNewResource(bCreateNewResource)
                .RunParser();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ListView? s = sender as ListView;
            if(listview != null)
            { 
            if (e.AddedItems.Count > 0)
            {

                ListViewItem item = listview.ItemContainerGenerator.ContainerFromIndex(listview.SelectedIndex) as ListViewItem;
                if (item != null)
                {
                    item.Focus();
                    item.BringIntoView();
                }
            }
            }
        }

        private void listview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 선택된 항목이 있다면
                if (listview.SelectedItem != null)
                {
                    // TextBox나 다른 컨트롤로 포커스를 이동시키기
                    Keyboard.Focus(projectTr2.txtE);
                }
            }
        }

        private void listview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 선택된 항목이 있다면
            if (listview.SelectedItem != null)
            {
                // TextBox나 다른 컨트롤로 포커스를 이동시키기
                Keyboard.Focus(projectTr2.txtE);
            }
        }
    }
}
