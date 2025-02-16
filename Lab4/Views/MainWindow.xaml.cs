using System.Collections.Generic;
using System.Windows;
using Lab4.Models;
using Lab4.Services;

namespace Lab4.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InputTextBox.Text = "do until a > 10 a = a + 1 loop";
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            StepsDataGrid.ItemsSource = null;

            string input = InputTextBox.Text;

            Lexer lexer = new Lexer();
            Token? tokens = lexer.LexAnalysis(input);

            Parser parser = new Parser(tokens);
            bool parseResult = parser.DoUntil(true);
            if (!parseResult)
            {
                MessageBox.Show("Ошибка синтаксического анализа");
                return;
            }

            StepInterpreter interpreter = new StepInterpreter(parser.Postfix, parser.Variables, parser.Constants);
            List<StepResult> steps = interpreter.ExecuteWithSteps();

            StepsDataGrid.ItemsSource = steps;
        }
    }
}