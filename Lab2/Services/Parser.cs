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

        public bool ParseWhileStatement()
        {
            // <WhileStatement> → while <Condition> do <Statement> end
            if (!Match(TokenType.While, "Ожидается 'while'")) return false;
            if (!ParseCondition()) return false;
            if (!Match(TokenType.Do, "Ожидается 'do'")) return false;
            if (!ParseStatement()) return false;
            if (!Match(TokenType.End, "Ожидается 'end'")) return false;

            if (_pos < _tokens.Count)
            {
                // Если остались лишние лексемы
                return Error("Лишние символы после 'end'");
            }
            return true;
        }

        private bool ParseCondition()
        {
            // Здесь должна быть логика разбора <Condition>
            // например, <LogExpr> { or <LogExpr> }
            // Упрощённо:
            if (!ParseLogExpr()) return false;
            while (Current != null && Current.Type == TokenType.Or)
            {
                NextToken();
                if (!ParseLogExpr()) return false;
            }
            return true;
        }

        private bool ParseLogExpr()
        {
            // <LogExpr> → <RelExpr> { and <RelExpr> }
            if (!ParseRelExpr()) return false;
            while (Current != null && Current.Type == TokenType.And)
            {
                NextToken();
                if (!ParseRelExpr()) return false;
            }
            return true;
        }

        private bool ParseRelExpr()
        {
            // <RelExpr> → <Operand> [rel <Operand>]
            if (!ParseOperand()) return false;

            if (Current != null && Current.Type == TokenType.Rel)
            {
                NextToken();
                if (!ParseOperand()) return false;
            }
            return true;
        }

        private bool ParseOperand()
        {
            // <Operand> → var | const
            if (Current == null) return Error("Ожидался операнд, но вход закончился");
            if (Current.Type == TokenType.Var || Current.Type == TokenType.Const)
            {
                NextToken();
                return true;
            }
            return Error("Ожидался var или const");
        }

        private bool ParseStatement()
        {
            // <Statement> → var as <ArithExpr>
            if (!Match(TokenType.Var, "Ожидался var")) return false;
            if (!Match(TokenType.As, "Ожидался 'as'")) return false;
            return ParseArithExpr();
        }

        private bool ParseArithExpr()
        {
            // <ArithExpr> → <Operand> { ao <Operand> }
            if (!ParseOperand()) return false;
            while (Current != null && Current.Type == TokenType.Ao)
            {
                NextToken();
                if (!ParseOperand()) return false;
            }
            return true;
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
