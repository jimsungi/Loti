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
    /// TextBoxExt.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TextBoxExt : UserControl
    {
        public TextBoxExt()
        {
            InitializeComponent();
            BigText = "pppp";
        }

        public static readonly DependencyProperty BigTextProperty = DependencyProperty.Register(
            "BigText", typeof(string), typeof(TextBoxExt), new PropertyMetadata(string.Empty));
        public string BigText
        {
            get { return (string)GetValue(BigTextProperty); }
            set { SetValue(BigTextProperty, value); }
        }
    }
}
