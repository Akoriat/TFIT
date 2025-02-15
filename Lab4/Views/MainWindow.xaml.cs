using System;
using System.Linq;
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
            InputTextBox.Text = "do until a < 10 a = a + 1; output a loop";
        }

        private void InterpretButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TokensListBox.Items.Clear();
                ParseLogTextBox.Clear();
                PostfixListBox.Items.Clear();
                InterpretOutputTextBox.Clear();

                var input = InputTextBox.Text;

                var lex = new LexAnalyzer();
                var lexOk = lex.Analyze(input);
                PostfixEntry.ConstTable = lex.Constants;
                PostfixEntry.IdentifierTable = lex.Identifiers;
                if (!lexOk)
                {
                    MessageBox.Show("Лексический анализ завершился с ошибками.");
                    return;
                }
                foreach (var token in lex.Tokens)
                {
                    TokensListBox.Items.Add(token.ToString());
                }

                var parseOk = Parser.Parse(lex.Tokens, lex.Identifiers, lex.Constants);
                ParseLogTextBox.Text = Parser.GetParseLog();
                if (!parseOk)
                {
                    MessageBox.Show("Синтаксический анализ завершился с ошибками.");
                    return;
                }
                var i = 0;
                foreach (var entry in Parser.Postfix)
                {
                    PostfixListBox.Items.Add($"{i++}: {entry}");
                }

                var interpResult = Interpreter.InterpretWithLogging(Parser.Postfix, lex.Identifiers, lex.Constants);
                InterpretOutputTextBox.Text = interpResult;

                MessageBox.Show("Анализ и интерпретация завершены успешно.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
