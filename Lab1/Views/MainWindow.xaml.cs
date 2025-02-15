using System.Windows;
using Lab1.Models;
using Lab1.Services;

namespace Lab1.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            InputTextBox.Text = "do until a < 10 and not b == 5 output a; loop";
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            TokensListBox.Items.Clear();
            ConstantsListBox.Items.Clear();
            IdentifiersListBox.Items.Clear();

            var input = InputTextBox.Text;
            Lexer lexer = new();
            var token = lexer.LexAnalysis(input);

            HashSet<string> constants = [];
            HashSet<string> identifiers = [];

            while (token != null)
            {
                var tokenInfo = $"Тип: {token.Type,-12}  Категория: {token.Category,-15}  Лексема: '{token.Lexeme}'  (позиция {token.Position})";
                TokensListBox.Items.Add(tokenInfo);

                if (token.Category == LexemeCategory.Constant)
                    constants.Add(token.Lexeme);
                else if (token.Category == LexemeCategory.Identifier)
                    identifiers.Add(token.Lexeme);

                token = token.Next;
            }

            foreach (var cons in constants)
                ConstantsListBox.Items.Add(cons);

            foreach (var ident in identifiers)
                IdentifiersListBox.Items.Add(ident);
        }
    }
}
