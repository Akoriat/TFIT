using System.Windows;
using Lab2.Models;
using Lab2.Services;

namespace Lab2.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InputTextBox.Text = "do until a < 10 and not b == 5 output a loop";
        }

        private void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            var input = InputTextBox.Text;

            Lexer lexer = new();
            var tokens = lexer.LexAnalysis(input);

            Parser parser = new(tokens);

            var result = parser.DoUntil(true);

            if (result)
            {
                ResultTextBlock.Text = "Анализ выполнен успешно.";
                ErrorsListBox.Items.Clear();
            }
            else
            {
                ResultTextBlock.Text = "Ошибка в анализе.";
                ErrorsListBox.Items.Clear();
                foreach (var err in parser.ErrorMessages)
                {
                    ErrorsListBox.Items.Add(err);
                }
            }
        }
    }
}