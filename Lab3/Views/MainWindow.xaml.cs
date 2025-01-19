using System;
using System.Windows;
using System.Collections.Generic;
using Lab3.Models;
using Lab3.Services;

namespace Lab3.Views
{
    public partial class MainWindow : Window
    {
        private Lexer _lexer;
        private IdentifierTable _idTable;
        private ConstantTable _constTable;

        public MainWindow()
        {
            InitializeComponent();

            _lexer = new Lexer();
            _idTable = new IdentifierTable();
            _constTable = new ConstantTable();
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Clear();
            string code = SourceTextBox.Text;

            // 1) Лексический анализ
            List<Token> tokens = _lexer.Analyze(code);
            // (при необходимости, присваиваем Index для var/const):
            foreach (var t in tokens)
            {
                if (t.Type == TokenType.Var)
                {
                    t.Index = _idTable.AddIdentifier(t.Lexeme);
                }
                else if (t.Type == TokenType.Const)
                {
                    t.Index = _constTable.AddConstant(t.Lexeme);
                }
            }

            // 2) Синтаксический анализ (рекурсивный спуск + ПОЛИЗ)
            var parser = new Parser(tokens, _idTable, _constTable);
            bool ok = parser.ParseWhileStatement();
            if (!ok)
            {
                ResultTextBox.Text = "Синтаксический анализ завершился с ошибками.\n";
                return;
            }

            // 3) Получаем ПОЛИЗ
            IReadOnlyList<PostfixEntry> postfix = parser.GetPostfix();

            // 4) Выводим результат
            ResultTextBox.Text = "Синтаксический анализ прошёл успешно!\nПОЛИЗ:\n";

            for (int i = 0; i < postfix.Count; i++)
            {
                var entry = postfix[i];
                ResultTextBox.Text += $"{i}\t";
                switch (entry.type)
                {
                    case EEntryType.etCmd:
                        // entry.index - код команды (ECmd)
                        ECmd cmd = (ECmd)entry.index;
                        ResultTextBox.Text += cmd.ToString();
                        break;

                    case EEntryType.etVar:
                        string varName = _idTable.GetIdentifier(entry.index);
                        ResultTextBox.Text += $"Var({varName})";
                        break;

                    case EEntryType.etConst:
                        string cstName = _constTable.GetConstant(entry.index);
                        ResultTextBox.Text += $"Const({cstName})";
                        break;

                    case EEntryType.etCmdPtr:
                        ResultTextBox.Text += $"CmdPtr({entry.index})";
                        break;
                }
                ResultTextBox.Text += "\n";
            }
        }
    }
}
