// Models/Node.cs

namespace Lab2.Models
{
    /// <summary>
    /// Базовый класс для всех узлов синтаксического дерева
    /// </summary>
    public abstract class Node
    {
        // При желании: позиция, вспомогательные методы, и т.д.
    }

    /// <summary>
    /// Узел для while-цикла
    /// </summary>
    public class WhileStatementNode : Node
    {
        public Node Condition { get; }
        public Node Statement { get; }

        public WhileStatementNode(Node condition, Node statement)
        {
            Condition = condition;
            Statement = statement;
        }
    }

    /// <summary>
    /// Блок/список операторов, если нужно
    /// </summary>
    public class StatementNode : Node
    {
        // Допустим, это просто "var as <expr>"
        public string VarName { get; }
        public Node Expression { get; }

        public StatementNode(string varName, Node expression)
        {
            VarName = varName;
            Expression = expression;
        }
    }

    /// <summary>
    /// Узел условного выражения (Condition)
    /// Здесь можно хранить список "LogExpr" с операцией "or"
    /// </summary>
    public class ConditionNode : Node
    {
        public Node Left { get; }
        public string Op { get; }
        public Node Right { get; }

        public ConditionNode(Node left, string op, Node right)
        {
            Left = left;
            Op = op;        // "or"
            Right = right;  // другой LogExpr
        }
    }

    /// <summary>
    /// Узел логического выражения (LogExpr),
    /// т.е. <RelExpr> and <RelExpr>...
    /// </summary>
    public class LogExprNode : Node
    {
        public Node Left { get; }
        public string Op { get; }
        public Node Right { get; }

        public LogExprNode(Node left, string op, Node right)
        {
            Left = left;
            Op = op;        // "and"
            Right = right;
        }
    }

    /// <summary>
    /// Выражение сравнения <RelExpr> = operand [ rel operand ]
    /// </summary>
    public class RelExprNode : Node
    {
        public Node Left { get; }
        public string RelOp { get; }
        public Node Right { get; }

        public RelExprNode(Node left, string relOp, Node right)
        {
            Left = left;
            RelOp = relOp;  // "<", ">", "<>", ...
            Right = right;
        }
    }

    /// <summary>
    /// Узел операнда (идентификатор или константа)
    /// </summary>
    public class OperandNode : Node
    {
        public string Value { get; }
        public bool IsVar { get; }

        public OperandNode(string value, bool isVar)
        {
            Value = value;
            IsVar = isVar;
        }
    }

    /// <summary>
    /// Узел арифметического выражения (ArithExpr).
    /// Например, left + right
    /// </summary>
    public class ArithExprNode : Node
    {
        public Node Left { get; }
        public string Ao { get; }
        public Node Right { get; }

        public ArithExprNode(Node left, string ao, Node right)
        {
            Left = left;
            Ao = ao;       // "+", "-", "*", "/"
            Right = right;
        }
    }
}
