using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public static readonly DependencyProperty TreeSourceProperty = DependencyProperty.Register(
            nameof(TreeSource), typeof(List<GoItemNode>), typeof(CheckFileTree), 
            new PropertyMetadata(new List<GoItemNode>()));
        public List<GoItemNode> TreeSource
        {
            get => (List<GoItemNode>)GetValue(TreeSourceProperty); 
            set => SetValue(TreeSourceProperty, value); 
        }
    }
}
