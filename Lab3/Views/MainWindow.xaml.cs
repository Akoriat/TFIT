using System;
using System.Collections.Generic;
using System.Windows;
using Lab3.Models;
using Lab3.Services;

namespace Lab3.Views
{
    public partial class MainWindow : Window
    {
        private Lexer _lexer;

        public MainWindow()
        {
            InitializeComponent();
            _lexer = new Lexer();
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

            // 1) Лексический анализ
            List<Token> tokens = _lexer.Analyze(code);

            ResultTextBox.Text = "=== Лексемы ===\n";
            foreach (var t in tokens)
            {
                ResultTextBox.Text += t.ToString() + "\n";
            }

            // 2) Синтаксический анализ => ПОЛИЗ
            var parser = new Parser(tokens);
            bool ok = parser.ParseProgram();
            if (!ok)
            {
                ResultTextBox.Text += "\nСинтаксический анализ завершился с ошибками.\n(См. вывод в консоли).";
                return;
            }

            // Получаем ПОЛИЗ
            var postfix = parser.GetPostfix();

            // Выводим ПОЛИЗ
            ResultTextBox.Text += "\n=== ПОЛИЗ ===\n";
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
                        ResultTextBox.Text += $"etVar({pe.index})";
                        break;
                    case EEntryType.etConst:
                        ResultTextBox.Text += $"etConst({pe.index})";
                        break;
                    case EEntryType.etCmdPtr:
                        ResultTextBox.Text += $"etCmdPtr({pe.index})";
                        break;
                }
                ResultTextBox.Text += "\n";
            }
        }
    }
}
