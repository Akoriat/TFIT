using System;
using System.Linq;
using System.Windows;
using Lab3.Models;
using Lab3.Services;

namespace Lab3.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InputTextBox.Text = "do until a < 10 a = a + 1 ; output a loop";
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TokensListBox.Items.Clear();
                ParseLogTextBox.Clear();
                PostfixListBox.Items.Clear();

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

                MessageBox.Show("Анализ завершён успешно.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
