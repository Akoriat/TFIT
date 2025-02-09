using System.Windows;

namespace LexicalAnalyzerWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Обработчик нажатия кнопки Analyze
        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            string input = InputTextBox.Text;
            LexAnalyzer analyzer = new LexAnalyzer();

            bool success = analyzer.Analyze(input);
            if (success)
            {
                // Отобразим результаты в DataGrid’ах
                TokensDataGrid.ItemsSource = analyzer.Tokens;
                IdentifiersDataGrid.ItemsSource = analyzer.Identifiers;
                ConstantsDataGrid.ItemsSource = analyzer.Constants;
            }
            else
            {
                MessageBox.Show("Lexical analysis failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
