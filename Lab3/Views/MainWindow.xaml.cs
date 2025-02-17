using System.Collections.Generic;
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
            InputTextBox.Text = "do until a < 10 and not b == 5 output a loop";
        }

        private void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorsListBox.Items.Clear();
            PolizItemsControl.ItemsSource = null;
            ResultTextBlock.Text = "";

            string input = InputTextBox.Text;

            Lexer lexer = new Lexer();
            Token? tokens = lexer.LexAnalysis(input);

            Parser parser = new Parser(tokens);

            bool result = parser.DoUntil(true);

            ResultTextBlock.Text = result ? "Анализ выполнен успешно." : "Ошибка в анализе.";

            foreach (var err in parser.ErrorMessages)
            {
                ErrorsListBox.Items.Add(err);
            }

            List<PolizDisplayItem> items = new List<PolizDisplayItem>();
            foreach (var entry in parser.Postfix)
            {
                string content;
                switch (entry.Type)
                {
                    case EEntryType.EtCmd:
                        content = ((ECmd)entry.Index).ToString();
                        break;
                    case EEntryType.EtVar:
                        content = "Var(" + parser.Variables[entry.Index] + ")";
                        break;
                    case EEntryType.EtConst:
                        content = "Const(" + parser.Constants[entry.Index] + ")";
                        break;
                    case EEntryType.EtCmdPtr:
                        content = "Ptr(" + entry.Index + ")";
                        break;
                    default:
                        content = "Unknown";
                        break;
                }
                items.Add(new PolizDisplayItem { Content = content });
            }

            PolizItemsControl.ItemsSource = items;
        }
    }

    public class PolizDisplayItem
    {
        public string Content { get; set; } = "";
    }
}