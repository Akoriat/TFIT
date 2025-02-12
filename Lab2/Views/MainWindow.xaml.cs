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
using Lab2.Models;
using Lab2.Services;

namespace Lab2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            string input = InputTextBox.Text;
            List<Token> tokenList;

            try
            {
                LexAnalyzer analyzer = new LexAnalyzer();
                analyzer.Analyze(input);
                tokenList = analyzer.Tokens;
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = "Ошибка лексического анализа: " + ex.Message;
                ResultTextBlock.Foreground = Brushes.Red;
                ParseLogTextBox.Text = "";
                return;
            }

            try
            {
                bool result = Parser.Parse(tokenList);
                if (result)
                {
                    ResultTextBlock.Text = "Синтаксический анализ завершён успешно.";
                    ResultTextBlock.Foreground = Brushes.Green;
                }
                else
                {
                    ResultTextBlock.Text = "Синтаксический анализ завершился с ошибками.";
                    ResultTextBlock.Foreground = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = "Ошибка синтаксического анализа: " + ex.Message;
                ResultTextBlock.Foreground = Brushes.Red;
            }

            ParseLogTextBox.Text = Parser.GetParseLog();
        }
    }
}