using System.Collections.Generic;

namespace Lab2.Models
{
    public abstract class Node
    {
        // Можно добавить поле Position, методы Accept(Visitor) и т.д.
    }

    public class WhileStatementNode : Node
    {
        public Node Condition { get; }
        public Node Statement { get; }

        public WhileStatementNode(Node cond, Node stmt)
        {
            Condition = cond;
            Statement = stmt;
        }
    }

    // И т.д. для других конструкций (ArithExprNode, OperandNode...)
}
