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
            var parser = new Parser(tokens);
            Node root = parser.ParseWhileStatement();
            if (root == null)
            {
                ResultTextBox.Text = "Синтаксический анализ завершился с ошибками.";
            }
            else
            {
                ResultTextBox.Text = "Синтаксический анализ прошёл успешно!\n";
                // Дополнительно, выводим структуру дерева
                string treeDump = DumpAST(root);
                ResultTextBox.Text += treeDump;
            }
        }

        private string DumpAST(Node node, int indent = 0)
        {
            if (node == null) return "";

            string pad = new string(' ', indent);
            switch (node)
            {
                case WhileStatementNode ws:
                    return pad + "WhileStatement\n" +
                           pad + "  Condition:\n" + DumpAST(ws.Condition, indent + 4) + "\n" +
                           pad + "  Statement:\n" + DumpAST(ws.Statement, indent + 4);

                case StatementNode st:
                    return pad + $"Statement (var={st.VarName})\n" +
                           pad + "  Expression:\n" + DumpAST(st.Expression, indent + 4);

                case ConditionNode cond:
                    return pad + $"Condition: Op={cond.Op}\n" +
                           pad + "  Left:\n" + DumpAST(cond.Left, indent + 4) + "\n" +
                           pad + "  Right:\n" + DumpAST(cond.Right, indent + 4);

                case LogExprNode lg:
                    return pad + $"LogExpr: Op={lg.Op}\n" +
                           pad + "  Left:\n" + DumpAST(lg.Left, indent + 4) + "\n" +
                           pad + "  Right:\n" + DumpAST(lg.Right, indent + 4);

                case RelExprNode re:
                    return pad + $"RelExpr: Op={re.RelOp}\n" +
                           pad + "  Left:\n" + DumpAST(re.Left, indent + 4) + "\n" +
                           pad + "  Right:\n" + DumpAST(re.Right, indent + 4);

                case OperandNode opn:
                    return pad + $"Operand: {opn.Value} (var={opn.IsVar})";

                case ArithExprNode ae:
                    return pad + $"ArithExpr: Op={ae.Ao}\n" +
                           pad + "  Left:\n" + DumpAST(ae.Left, indent + 4) + "\n" +
                           pad + "  Right:\n" + DumpAST(ae.Right, indent + 4);

                default:
                    return pad + $"Unknown node type: {node.GetType()}";
            }
        }

    }
}
