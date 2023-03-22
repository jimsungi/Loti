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
using System.Windows.Shapes;

namespace TigerL10N.Views
{
    /// <summary>
    /// HelpDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HelpDialog : Window
    {
        public HelpDialog()
        {
            InitializeComponent();
            ////this.Title = "---";

            ////try
            ////{
            ////    // Load HTML document as a stream
            ////    Uri uri = new Uri(@"pack://application:,,,/" + "readme.txt", UriKind.Absolute);
            ////    Stream source = Application.GetResourceStream(uri).Stream;

            ////    // Navigate to HTML document stream
            ////    this.webBrowser.NavigateToStream(source);
            ////}
            ////catch
            ////{

            ////}
        }

        private void webBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            //if (e.Uri == null)
            //{
            //    return;
            //}
            //string url = e.Uri.ToString();

            //if (url.StartsWith("about:") && url != "about:blank")
            //{
            //    this.webBrowser.NavigateToStream(Application.GetResourceStream(e.Uri).Stream);
            //}
            //else if (url.StartsWith("http"))
            //{
            //    // Display external links using default webbrowser
            //    e.Cancel = true;
            //    System.Diagnostics.Process.Start(url);
            //}
        }
    }
}
