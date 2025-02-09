using System;
using System.Collections.Generic;
using Lab2.Models;

namespace Lab2.Services
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
        }

        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : null;

        private void NextToken()
        {
            if (_pos < _tokens.Count) _pos++;
        }

        private bool Error(string message)
        {
            if (Current != null)
            {
                Console.WriteLine($"Ошибка: {message}. Позиция {Current.Position}, лексема: {Current.Lexeme}");
            }
            else
            {
                Console.WriteLine($"Ошибка: {message}. (конец лексем)");
            }
            return false;
        }

        public Node ParseWhileStatement()
        {
            // <WhileStatement> → while <Condition> do <Statement> end

            // 1) Проверяем 'while'
            if (!Match(TokenType.While, "Ожидался 'while'")) return null;

            // 2) Парсим Condition
            Node condition = ParseCondition();
            if (condition == null) return null;

            // 3) do
            if (!Match(TokenType.Do, "Ожидался 'do'")) return null;

            // 4) Statement
            Node stmt = ParseStatement();
            if (stmt == null) return null;

            // 5) end
            if (!Match(TokenType.End, "Ожидался 'end'")) return null;

            // Если остались лишние лексемы...
            if (_pos < _tokens.Count)
            {
                Error("Лишние символы после 'end'");
                return null;
            }

            // Возвращаем узел WhileStatementNode
            return new WhileStatementNode(condition, stmt);
        }


        private Node ParseCondition()
        {
            // По EBNF: <Condition> → <LogExpr> { or <LogExpr> }
            Node left = ParseLogExpr();
            if (left == null) return null;

            // Если встретили `or`, делаем узел ConditionNode c Left=left, Right=ParseLogExpr
            while (Current != null && Current.Type == TokenType.Or)
            {
                NextToken(); // съели 'or'
                Node right = ParseLogExpr();
                if (right == null) return null;

                // Создаём новый узел (left or right)
                left = new ConditionNode(left, "or", right);
            }

            return left;
        }


        private Node ParseLogExpr()
        {
            // <LogExpr> → <RelExpr> { and <RelExpr> }
            Node left = ParseRelExpr();
            if (left == null) return null;

            while (Current != null && Current.Type == TokenType.And)
            {
                NextToken(); // 'and'
                Node right = ParseRelExpr();
                if (right == null) return null;

                left = new LogExprNode(left, "and", right);
            }
            return left;
        }


        private Node ParseRelExpr()
        {
            // <RelExpr> → <Operand> [ rel <Operand> ]
            Node left = ParseOperand();
            if (left == null) return null;

            if (Current != null && Current.Type == TokenType.Rel)
            {
                string op = Current.Lexeme; // например "<", ">", "<>"
                NextToken();
                Node right = ParseOperand();
                if (right == null) return null;

                return new RelExprNode(left, op, right);
            }
            // иначе просто left
            return left;
        }


        private Node ParseOperand()
        {
            if (Current == null)
            {
                Error("Ожидался операнд, но вход закончился");
                return null;
            }

            if (Current.Type == TokenType.Var)
            {
                string name = Current.Lexeme;
                int pos = Current.Position;
                NextToken();
                return new OperandNode(name, isVar: true);
            }
            else if (Current.Type == TokenType.Const)
            {
                string val = Current.Lexeme;
                int pos = Current.Position;
                NextToken();
                return new OperandNode(val, isVar: false);
            }

            Error("Ожидался var или const");
            return null;
        }


        private Node ParseStatement()
        {
            if (Current == null || Current.Type != TokenType.Var)
            {
                Error("Ожидался идентификатор (var)");
                return null;
            }
            string varName = Current.Lexeme;
            NextToken();

            if (!Match(TokenType.As, "Ожидался 'as'")) return null;

            Node expr = ParseArithExpr();
            if (expr == null) return null;

            return new StatementNode(varName, expr);
        }


        private Node ParseArithExpr()
        {
            Node left = ParseOperand();
            if (left == null) return null;

            // цикл, пока видим ao
            while (Current != null && Current.Type == TokenType.Ao)
            {
                string op = Current.Lexeme; // например "+"
                NextToken();
                Node right = ParseOperand();
                if (right == null) return null;

                // создаём узел бинарной операции: left op right
                left = new ArithExprNode(left, op, right);
            }

            return left;
        }


        private bool Match(TokenType type, string errMessage)
        {
            if (Current == null || Current.Type != type)
                return Error(errMessage);
            NextToken();
            return true;
        }
    }
}
