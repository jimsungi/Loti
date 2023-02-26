using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static TigerL10N.ViewModels.MainWindowViewModel;

namespace TigerL10N.Control
{
    /// <summary>
    /// TransEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TransEditor : System.Windows.Controls.UserControl
    {
        public TransEditor()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty WordProperty = DependencyProperty.Register(
            nameof(Word), typeof(WordItem), typeof(TransEditor),
            new PropertyMetadata(new WordItem()));
        public WordItem Word
        {
            get => (WordItem)GetValue(WordProperty);
            set => SetValue(WordProperty, value);
        }

        public static readonly DependencyProperty WorderProperty = DependencyProperty.Register(
    nameof(Worder), typeof(DelegateCommand), typeof(TransEditor),
    new PropertyMetadata(new DelegateCommand(DymmyCmd)));
        public DelegateCommand Worder
        {
            get => (DelegateCommand)GetValue(WorderProperty);
            set => SetValue(WorderProperty, value);
        }

        static void DymmyCmd()
        {

            // throw new NotImplementException();
        }

        private DelegateCommand<string>? _specialKeyCmd = null;
        public DelegateCommand<string> SpecialKeyCmd =>
            _specialKeyCmd ??= new DelegateCommand<string>(SpecialKeyFunc);
        void SpecialKeyFunc(string param)
        {
            if(param == "Shift+Enter")
            {
                System.Windows.Forms.MessageBox.Show("ssss");
            }
            // throw new NotImplementException();
        }

        private void txtE_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                //System.Windows.Forms.MessageBox.Show("Enter + Shift");
                MyEventArgs args = new MyEventArgs(MyEvent, this, "Hello, world!");
                Worder.Execute();
                RaiseEvent(args);
            }
        }

        public static readonly RoutedEvent MyEvent = EventManager.RegisterRoutedEvent(
      "My", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TransEditor));

        public event RoutedEventHandler My
        {
            add { AddHandler(MyEvent, value); }
            remove { RemoveHandler(MyEvent, value); }
        }
    }

    public class KeyEventArgsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is System.Windows.Input.KeyEventArgs args && args.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                return "Shift+Enter";
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MyEventArgs : RoutedEventArgs
    {
        public string Message { get; set; }

        public MyEventArgs(RoutedEvent routedEvent, object source, string message)
            : base(routedEvent, source)
        {
            Message = message;
        }
    }
}
