using System;
using System.Collections.Generic;
using System.Text;
using Lab2.Models;

namespace Lab2.Services
{
    public static class Parser
    {
        private static List<Token> tokens;

        private static int currentIndex;

        private static StringBuilder parseLog = new StringBuilder();
        private static int indentLevel = 0;

        private static Token CurrentToken => currentIndex < tokens.Count ? tokens[currentIndex] : null;

        private static void Log(string message)
        {
            parseLog.AppendLine(new string(' ', indentLevel * 2) + message);
        }

        public static string GetParseLog()
        {
            return parseLog.ToString();
        }

        private static void Error(string msg, int pos)
        {
            throw new Exception($"Ошибка на позиции {pos}: {msg}");
        }

        public static bool DoUntilStatement()
        {
            Log("Вход в DoUntilStatement, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (CurrentToken == null || CurrentToken.Type != TokenType.Do)
            {
                Error("Ожидалось 'do'", CurrentToken != null ? CurrentToken.StartPos : -1);
                return false;
            }
            currentIndex++;

            if (CurrentToken == null || CurrentToken.Type != TokenType.Until)
            {
                Error("Ожидалось 'until'", CurrentToken != null ? CurrentToken.StartPos : -1);
                return false;
            }
            currentIndex++;

            if (!LogicalExpression())
                return false;

            if (!Operators())
                return false;

            if (CurrentToken == null || CurrentToken.Type != TokenType.Loop)
            {
                Error("Ожидалось 'loop'", CurrentToken != null ? CurrentToken.StartPos : -1);
                return false;
            }
            currentIndex++;

            indentLevel--;
            Log("Выход из DoUntilStatement, результат: true, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            if (currentIndex < tokens.Count)
            {
                Error("Лишние токены после 'loop'", CurrentToken != null ? CurrentToken.StartPos : -1);
                return false;
            }
            return true;
        }

        public static bool LogicalExpression()
        {
            Log("Вход в LogicalExpression, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;

            if (CurrentToken != null && CurrentToken.Type == TokenType.Not)
            {
                Log("Найден 'not'");
                currentIndex++;
            }

            if (!ComparisonExpression())
                return false;

            while (CurrentToken != null &&
                   (CurrentToken.Type == TokenType.And || CurrentToken.Type == TokenType.Or))
            {
                Log("Найден логический оператор: " + CurrentToken.ToString());
                currentIndex++;

                if (CurrentToken != null && CurrentToken.Type == TokenType.Not)
                {
                    Log("Найден 'not' после логического оператора");
                    currentIndex++;
                }

                if (!ComparisonExpression())
                    return false;
            }
            indentLevel--;
            Log("Выход из LogicalExpression, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }
        public static bool ComparisonExpression()
        {
            Log("Вход в ComparisonExpression, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (!Operand())
                return false;

            if (CurrentToken != null &&
                CurrentToken.Type == TokenType.Rel)
            {
                Log("Найден оператор сравнения: " + CurrentToken.ToString());
                currentIndex++;
                if (!Operand())
                    return false;
            }
            indentLevel--;
            Log("Выход из ComparisonExpression, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        public static bool Operand()
        {
            Log("Вход в Operand, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (CurrentToken == null)
            {
                Error("Ожидался операнд (идентификатор или константа)", -1);
                return false;
            }
            if (CurrentToken.Type == TokenType.Identifier || CurrentToken.Type == TokenType.Constant)
            {
                Log("Найден операнд: " + CurrentToken.ToString());
                currentIndex++;
                indentLevel--;
                Log("Выход из Operand, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
                return true;
            }
            else
            {
                Error("Ожидался идентификатор или константа", CurrentToken.StartPos);
                return false;
            }
        }

        public static bool ArithExpr()
        {
            Log("Вход в ArithExpr, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (!Operand())
                return false;

            while (CurrentToken != null &&
                   (CurrentToken.Type == TokenType.Plus ||
                    CurrentToken.Type == TokenType.Minus ||
                    CurrentToken.Type == TokenType.Multiply ||
                    CurrentToken.Type == TokenType.Divide))
            {
                Log("Найден арифметический оператор: " + CurrentToken.ToString());
                currentIndex++;
                if (!Operand())
                    return false;
            }
            indentLevel--;
            Log("Выход из ArithExpr, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        public static bool Operators()
        {
            Log("Вход в Operators, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (!OperatorStmt())
                return false;

            while (CurrentToken != null && CurrentToken.Type == TokenType.Delimiter)
            {
                Log("Найден символ ';'");
                currentIndex++;
                if (!OperatorStmt())
                    return false;
            }
            indentLevel--;
            Log("Выход из Operators, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        public static bool OperatorStmt()
        {
            Log("Вход в OperatorStmt, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (CurrentToken == null)
            {
                Error("Ожидался оператор", -1);
                return false;
            }
            if (CurrentToken.Type == TokenType.Identifier)
            {
                Log("Обнаружено присваивание, идентификатор: " + CurrentToken.ToString());
                currentIndex++;
                if (CurrentToken == null || CurrentToken.Type != TokenType.As)
                {
                    Error("Ожидалось '=' в присваивании", CurrentToken != null ? CurrentToken.StartPos : -1);
                    return false;
                }
                Log("Найден '='");
                currentIndex++;
                if (!ArithExpr())
                    return false;
                indentLevel--;
                Log("Выход из OperatorStmt (присваивание), текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
                return true;
            }
            else if (CurrentToken.Type == TokenType.Output)
            {
                Log("Обнаружен оператор вывода: " + CurrentToken.ToString());
                currentIndex++;
                if (!Operand())
                    return false;
                indentLevel--;
                Log("Выход из OperatorStmt (вывод), текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
                return true;
            }
            else
            {
                Error("Ожидалось присваивание или оператор вывода", CurrentToken.StartPos);
                return false;
            }
        }

        public static bool Parse(List<Token> tokenList)
        {
            tokens = tokenList;
            currentIndex = 0;
            parseLog.Clear();
            indentLevel = 0;
            Log("=== Начало синтаксического анализа ===");
            bool result = DoUntilStatement();
            Log("=== Конец синтаксического анализа ===");
            return result;
        }
    }
}
