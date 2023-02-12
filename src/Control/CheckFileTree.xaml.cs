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

namespace TigerL10N.Control
{
    /// <summary>
    /// CheckFileTree.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CheckFileTree : UserControl
    {
        public CheckFileTree()
        {
            InitializeComponent();
        }

        public void SetItem()
        {
            this.thisTree.ItemsSource = CheckFileTreeModel.SetTree("Top Level");
        }
    }
}
