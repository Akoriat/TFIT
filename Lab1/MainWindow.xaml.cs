using Lab1.ViewModels;
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

namespace Lab1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.InputText = InputTextBox.Text;
            _viewModel.Analyze();
        }

        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == "Введите текст для анализа...")
            {
                InputTextBox.Text = "";
                InputTextBox.Foreground = Brushes.Black; // Меняем цвет текста на стандартный
            }
        }

        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                InputTextBox.Text = "Введите текст для анализа...";
                InputTextBox.Foreground = Brushes.Gray; // Меняем цвет текста на серый для подсказки
            }
        }
    }

}