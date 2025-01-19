using System;
using System.Collections.Generic;
using System.Windows;
using Lab4.Models;
using Lab4.Services;

namespace Lab4.Views
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

            string code = SourceTextBox.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                ResultTextBox.Text = "Введите исходный код.";
                return;
            }

            try
            {
                // 1) Лексический анализ
                List<Token> tokens = _lexer.Analyze(code);

                // Заполняем Index для var/const
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

                // 2) Синтаксический анализ (построение ПОЛИЗ)
                var parser = new Parser(tokens, _idTable, _constTable);
                bool ok = parser.ParseWhileStatement();
                // Или ParseProgram() - смотря как у вас называется

                if (!ok)
                {
                    ResultTextBox.Text = "Синтаксический анализ завершился с ошибками. См. консоль.";
                    return;
                }

                IReadOnlyList<PostfixEntry> postfix = parser.GetPostfix();

                // 3) Интерпретация
                int varCount = _idTable.All.Count;
                var interpreter = new Interpreter(new List<PostfixEntry>(postfix), varCount, _constTable);

                // Например, если хотим инициализировать переменную a=0, 
                // можно найти индекс 'a' и сделать:
                // int aIndex = _idTable.All.IndexOf("a");
                // if (aIndex >= 0) interpreter.SetVarValue(aIndex, 0);

                // Запуск
                interpreter.Run();

                // Выводим ПОЛИЗ
                ResultTextBox.Text = "Анализ прошёл успешно!\nПОЛИЗ:\n";
                for (int i = 0; i < postfix.Count; i++)
                {
                    var pe = postfix[i];
                    ResultTextBox.Text += $"{i}: ";
                    switch (pe.type)
                    {
                        case EEntryType.etCmd:
                            {
                                ECmd cmd = (ECmd)pe.index;
                                ResultTextBox.Text += cmd.ToString();
                            }
                            break;
                        case EEntryType.etVar:
                            {
                                string vName = _idTable.GetIdentifier(pe.index);
                                ResultTextBox.Text += $"Var({vName})";
                            }
                            break;
                        case EEntryType.etConst:
                            {
                                string cVal = _constTable.GetConstant(pe.index);
                                ResultTextBox.Text += $"Const({cVal})";
                            }
                            break;
                        case EEntryType.etCmdPtr:
                            {
                                ResultTextBox.Text += $"CmdPtr({pe.index})";
                            }
                            break;
                    }
                    ResultTextBox.Text += "\n";
                }

                // Выводим значения всех переменных
                ResultTextBox.Text += "\nЗначения переменных:\n";
                for (int i = 0; i < varCount; i++)
                {
                    string name = _idTable.All[i];
                    int value = interpreter.GetVarValue(i);
                    ResultTextBox.Text += $"{name} = {value}\n";
                }
            }
            catch (Exception ex)
            {
                ResultTextBox.Text = $"Ошибка: {ex.Message}\n{ex.StackTrace}";
            }
        }
    }
}
