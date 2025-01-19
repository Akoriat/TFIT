using System;
using System.Windows;
using Lab2.Models;
using Lab2.Services;
using System.Collections.Generic;

namespace Lab2.Views
{
    public partial class MainWindow : Window
    {
        private Lexer _lexer;   // опционально, если хотим вызывать lexer
        private Parser _parser;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация, если нужно
            _lexer = new Lexer();
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Clear();
            string code = SourceCodeTextBox.Text;

            // 1) Лексический анализ (опционально — или берём готовый список токенов)
            List<Token> tokens = _lexer.Analyze(code);

            // 2) Синтаксический анализ
            _parser = new Parser(tokens);
            bool res = _parser.ParseWhileStatement();

            // 3) Вывод результата
            if (res)
                ResultTextBox.Text = "Синтаксический анализ прошёл успешно!";
            else
                ResultTextBox.Text += "\r\nСинтаксический анализ завершился с ошибками.";
        }
    }
}
