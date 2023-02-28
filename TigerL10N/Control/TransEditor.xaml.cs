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

        #region Registered Delegate Command Property
        public static readonly DependencyProperty ApplyAllCmdProperty = DependencyProperty.Register(
    nameof(ApplyAllCmd), typeof(DelegateCommand), typeof(TransEditor),
    new PropertyMetadata(new DelegateCommand(DummyCmdProperty)));
        public DelegateCommand ApplyAllCmd
        {
            get => (DelegateCommand)GetValue(ApplyAllCmdProperty);
            set => SetValue(ApplyAllCmdProperty, value);
        }

        public static readonly DependencyProperty IgnoreCmdProperty = DependencyProperty.Register(
nameof(IgnoreCmd), typeof(DelegateCommand), typeof(TransEditor),
new PropertyMetadata(new DelegateCommand(DummyCmdProperty)));
        public DelegateCommand IgnoreCmd
        {
            get => (DelegateCommand)GetValue(IgnoreCmdProperty);
            set => SetValue(IgnoreCmdProperty, value);
        }

        public static readonly DependencyProperty AsIdCmdProperty = DependencyProperty.Register(
nameof(AsIdCmd), typeof(DelegateCommand), typeof(TransEditor),
new PropertyMetadata(new DelegateCommand(DummyCmdProperty)));
        public DelegateCommand AsIdCmd
        {
            get => (DelegateCommand)GetValue(AsIdCmdProperty);
            set => SetValue(AsIdCmdProperty, value);
        }

        public static readonly DependencyProperty NextCmdProperty = DependencyProperty.Register(
nameof(NextCmd), typeof(DelegateCommand), typeof(TransEditor),
new PropertyMetadata(new DelegateCommand(DummyCmdProperty)));
        public DelegateCommand NextCmd
        {
            get => (DelegateCommand)GetValue(NextCmdProperty);
            set => SetValue(NextCmdProperty, value);
        }

        public static readonly DependencyProperty PrevCmdProperty = DependencyProperty.Register(
nameof(PrevCmd), typeof(DelegateCommand), typeof(TransEditor),
new PropertyMetadata(new DelegateCommand(DummyCmdProperty)));
        public DelegateCommand PrevCmd
        {
            get => (DelegateCommand)GetValue(PrevCmdProperty);
            set => SetValue(PrevCmdProperty, value);
        }

        static void DummyCmdProperty()
        {
            // throw new NotImplementException();
        }
        #endregion


        private void txtE_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Word == null)
                return;
            if(e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                NextCmd.Execute();
                e.Handled = true;
            }
            else if(e.Key == Key.Down)
            {
                NextCmd.Execute();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                PrevCmd.Execute();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && !Word.SourceString.Contains("\r\n"))
            {
                NextCmd.Execute();
                e.Handled = true;
            }
            else if(e.Key== Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                ApplyAllCmd.Execute();
                e.Handled = true;
            }
            else if (e.Key == Key.I && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                IgnoreCmd.Execute();
                e.Handled = true;
            }
            else if (e.Key == Key.U && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                AsIdCmd.Execute();
                e.Handled = true;
            }

        }



        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Apply All(Ctrl+A)<

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

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
