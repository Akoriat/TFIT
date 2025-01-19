using System.Windows;
using Lab1.Services;
using Lab1.Models;
using System.Collections.Generic;

namespace Lab1.Views
{
    public partial class MainWindow : Window
    {
        private readonly Lexer _lexer;
        private readonly IdentifierTable _idTable;
        private readonly ConstantTable _constTable;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализируем таблицы и лексер
            _idTable = new IdentifierTable();
            _constTable = new ConstantTable();
            _lexer = new Lexer(_idTable, _constTable);
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            TokensListBox.Items.Clear();

            string code = SourceCodeTextBox.Text;

            // Запускаем лексический анализ
            List<Token> tokens = _lexer.Analyze(code);

            // Выводим лексемы
            foreach (var token in tokens)
            {
                TokensListBox.Items.Add(token.ToString());
            }
        }
    }
}

