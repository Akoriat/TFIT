using System;
using System.Collections.Generic;
using System.Text;
using Lab2.Models;

namespace Lab2.Services
{
    // Синтаксический анализатор методом рекурсивного спуска
    public static class Parser
    {
        private static List<Token> tokens;
        private static int currentIndex;

        // Строковый буфер для лога этапов разбора
        private static StringBuilder parseLog = new StringBuilder();
        private static int indentLevel = 0;

        // Текущий токен или null, если достигнут конец
        private static Token CurrentToken => currentIndex < tokens.Count ? tokens[currentIndex] : null;

        // Вспомогательный метод для ведения лога с отступами
        private static void Log(string message)
        {
            parseLog.AppendLine(new string(' ', indentLevel * 2) + message);
        }

        // Метод для получения лога разбора
        public static string GetParseLog()
        {
            return parseLog.ToString();
        }

        // Метод для генерации ошибки с выбросом исключения
        private static void Error(string msg, int pos)
        {
            throw new Exception($"Ошибка на позиции {pos}: {msg}");
        }

        /// <summary>
        /// <DoUntilStatement> → do until <LogicalExpression> <Operators> loop
        /// </summary>
        public static bool DoUntilStatement()
        {
            Log("Вход в DoUntilStatement, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (CurrentToken == null || CurrentToken.Type != TokenType.lDo)
            {
                Error("Ожидалось 'do'", CurrentToken != null ? CurrentToken.Position : -1);
                return false;
            }
            currentIndex++; // потребляем 'do'

            if (CurrentToken == null || CurrentToken.Type != TokenType.lUntil)
            {
                Error("Ожидалось 'until'", CurrentToken != null ? CurrentToken.Position : -1);
                return false;
            }
            currentIndex++; // потребляем 'until'

            if (!LogicalExpression())
                return false;

            if (!Operators())
                return false;

            if (CurrentToken == null || CurrentToken.Type != TokenType.lLoop)
            {
                Error("Ожидалось 'loop'", CurrentToken != null ? CurrentToken.Position : -1);
                return false;
            }
            currentIndex++; // потребляем 'loop'

            indentLevel--;
            Log("Выход из DoUntilStatement, результат: true, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            if (currentIndex < tokens.Count)
            {
                Error("Лишние токены после 'loop'", CurrentToken != null ? CurrentToken.Position : -1);
                return false;
            }
            return true;
        }

        /// <summary>
        /// <LogicalExpression> → [ not ] <ComparisonExpression> { (and | or) [ not ] <ComparisonExpression> }
        /// </summary>
        public static bool LogicalExpression()
        {
            Log("Вход в LogicalExpression, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            // Если встречается "not", потребляем его (необязательно)
            if (CurrentToken != null && CurrentToken.Type == TokenType.lNot)
            {
                Log("Найден 'not'");
                currentIndex++;
            }

            if (!ComparisonExpression())
                return false;

            // Обработка бинарных логических операций (and/or)
            while (CurrentToken != null &&
                   (CurrentToken.Type == TokenType.lAnd || CurrentToken.Type == TokenType.lOr))
            {
                Log("Найден логический оператор: " + CurrentToken.ToString());
                currentIndex++; // потребляем and/or

                // Если после логической операции идёт "not", потребляем его
                if (CurrentToken != null && CurrentToken.Type == TokenType.lNot)
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

        /// <summary>
        /// <ComparisonExpression> → <Operand> [ (< | > | = | <>) <Operand> ]
        /// </summary>
        public static bool ComparisonExpression()
        {
            Log("Вход в ComparisonExpression, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (!Operand())
                return false;

            if (CurrentToken != null &&
                (CurrentToken.Type == TokenType.lLess ||
                 CurrentToken.Type == TokenType.lGreater ||
                 CurrentToken.Type == TokenType.lEqual ||
                 CurrentToken.Type == TokenType.lNotEqual))
            {
                Log("Найден оператор сравнения: " + CurrentToken.ToString());
                currentIndex++; // потребляем оператор сравнения
                if (!Operand())
                    return false;
            }
            indentLevel--;
            Log("Выход из ComparisonExpression, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        /// <summary>
        /// <Operand> → var | const
        /// </summary>
        public static bool Operand()
        {
            Log("Вход в Operand, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (CurrentToken == null)
            {
                Error("Ожидался операнд (идентификатор или константа)", -1);
                return false;
            }
            if (CurrentToken.Type == TokenType.lVar || CurrentToken.Type == TokenType.lConst)
            {
                Log("Найден операнд: " + CurrentToken.ToString());
                currentIndex++; // потребляем операнд
                indentLevel--;
                Log("Выход из Operand, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
                return true;
            }
            else
            {
                Error("Ожидался идентификатор или константа", CurrentToken.Position);
                return false;
            }
        }

        /// <summary>
        /// <ArithExpr> → <Operand> { (+ | - | * | /) <Operand> }
        /// </summary>
        public static bool ArithExpr()
        {
            Log("Вход в ArithExpr, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (!Operand())
                return false;

            while (CurrentToken != null &&
                   (CurrentToken.Type == TokenType.lPlus ||
                    CurrentToken.Type == TokenType.lMinus ||
                    CurrentToken.Type == TokenType.lMul ||
                    CurrentToken.Type == TokenType.lDiv))
            {
                Log("Найден арифметический оператор: " + CurrentToken.ToString());
                currentIndex++; // потребляем арифметический оператор
                if (!Operand())
                    return false;
            }
            indentLevel--;
            Log("Выход из ArithExpr, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        /// <summary>
        /// <Operators> → <Operator> { ; <Operator> }
        /// </summary>
        public static bool Operators()
        {
            Log("Вход в Operators, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (!OperatorStmt())
                return false;

            while (CurrentToken != null && CurrentToken.Type == TokenType.lSemicolon)
            {
                Log("Найден символ ';'");
                currentIndex++; // потребляем ';'
                if (!OperatorStmt())
                    return false;
            }
            indentLevel--;
            Log("Выход из Operators, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        /// <summary>
        /// <Operator> → <идентификатор> = <ArithExpr> | output <Operand>
        /// </summary>
        public static bool OperatorStmt()
        {
            Log("Вход в OperatorStmt, текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (CurrentToken == null)
            {
                Error("Ожидался оператор", -1);
                return false;
            }
            // Если оператор начинается с идентификатора – это присваивание
            if (CurrentToken.Type == TokenType.lVar)
            {
                Log("Обнаружено присваивание, идентификатор: " + CurrentToken.ToString());
                currentIndex++; // потребляем идентификатор
                if (CurrentToken == null || CurrentToken.Type != TokenType.lEqual)
                {
                    Error("Ожидалось '=' в присваивании", CurrentToken != null ? CurrentToken.Position : -1);
                    return false;
                }
                Log("Найден '='");
                currentIndex++; // потребляем '='
                if (!ArithExpr())
                    return false;
                indentLevel--;
                Log("Выход из OperatorStmt (присваивание), текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
                return true;
            }
            // Если оператор начинается с "output"
            else if (CurrentToken.Type == TokenType.lOutput)
            {
                Log("Обнаружен оператор вывода: " + CurrentToken.ToString());
                currentIndex++; // потребляем 'output'
                if (!Operand())
                    return false;
                indentLevel--;
                Log("Выход из OperatorStmt (вывод), текущий токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
                return true;
            }
            else
            {
                Error("Ожидалось присваивание или оператор вывода", CurrentToken.Position);
                return false;
            }
        }

        /// <summary>
        /// Точка входа синтаксического анализатора.
        /// </summary>
        public static bool Parse(List<Token> tokenList)
        {
            // Инициализация указателя и лога
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
