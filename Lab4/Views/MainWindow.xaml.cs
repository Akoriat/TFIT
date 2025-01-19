﻿using System;
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

                // Проставляем index для Var/Const
                foreach (var t in tokens)
                {
                    if (t.Type == TokenType.Var)
                        t.Index = _idTable.AddIdentifier(t.Lexeme);
                    else if (t.Type == TokenType.Const)
                        t.Index = _constTable.AddConstant(t.Lexeme);
                }

                // 2) Синтаксический анализ => ПОЛИЗ
                var parser = new Parser(tokens, _idTable, _constTable);
                bool ok = parser.ParseProgram();  // парсим 1 оператор

                if (!ok)
                {
                    ResultTextBox.Text = "Синтаксический анализ завершился с ошибками (см. консоль).";
                    return;
                }

                // Получаем ПОЛИЗ
                var postfix = parser.GetPostfix();

                // 3) Интерпретация
                int varCount = _idTable.All.Count;
                var interpreter = new Interpreter(new List<PostfixEntry>(postfix), varCount, _constTable);

                interpreter.Run();

                // Печатаем ПОЛИЗ
                ResultTextBox.Text = "Анализ прошёл успешно!\n=== ПОЛИЗ: ===\n";
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
                                string vname = _idTable.GetIdentifier(pe.index);
                                ResultTextBox.Text += $"etVar({vname})";
                            }
                            break;
                        case EEntryType.etConst:
                            {
                                string cVal = _constTable.GetConstant(pe.index);
                                ResultTextBox.Text += $"etConst({cVal})";
                            }
                            break;
                        case EEntryType.etCmdPtr:
                            {
                                ResultTextBox.Text += $"etCmdPtr({pe.index})";
                            }
                            break;
                    }
                    ResultTextBox.Text += "\n";
                }

                // Выводим значения переменных
                ResultTextBox.Text += "\n=== Значения переменных ===\n";
                for (int i = 0; i < varCount; i++)
                {
                    string name = _idTable.All[i];
                    int val = interpreter.GetVarValue(i);
                    ResultTextBox.Text += $"{name} = {val}\n";
                }
            }
            catch (Exception ex)
            {
                ResultTextBox.Text = $"Ошибка:\n{ex.Message}\n{ex.StackTrace}";
            }
        }
    }
}
