using System;
using System.Collections.Generic;
using System.Text;
using Lab4.Models;

namespace Lab4.Services
{
    public static class Parser
    {
        private static List<Token> tokens;
        private static int currentIndex;
        private static StringBuilder parseLog = new StringBuilder();
        private static int indentLevel = 0;

        public static List<PostfixEntry> Postfix { get; private set; } = new List<PostfixEntry>();
        public static List<string> Identifiers { get; set; }
        public static List<string> Constants { get; set; }

        private static Token CurrentToken => currentIndex < tokens.Count ? tokens[currentIndex] : null;
        public static string GetParseLog() => parseLog.ToString();

        private static void Log(string message)
        {
            parseLog.AppendLine(new string(' ', indentLevel * 2) + message);
        }

        private static void Error(string msg, int pos)
        {
            throw new Exception($"Ошибка на позиции {pos}: {msg}");
        }

        private static int WriteCmd(ECmd cmd)
        {
            Postfix.Add(new PostfixEntry { Type = EEntryType.etCmd, Index = (int)cmd });
            return Postfix.Count - 1;
        }

        private static int WriteVar(int varIndex)
        {
            Postfix.Add(new PostfixEntry { Type = EEntryType.etVar, Index = varIndex });
            return Postfix.Count - 1;
        }

        private static int WriteConst(int constIndex)
        {
            Postfix.Add(new PostfixEntry { Type = EEntryType.etConst, Index = constIndex });
            return Postfix.Count - 1;
        }

        private static int WriteCmdPtr(int ptr)
        {
            Postfix.Add(new PostfixEntry { Type = EEntryType.etCmdPtr, Index = ptr });
            return Postfix.Count - 1;
        }

        private static void SetCmdPtr(int ind, int ptr)
        {
            Postfix[ind] = new PostfixEntry { Type = EEntryType.etCmdPtr, Index = ptr };
        }

        
        private static void SwapLastTwoEntries() // для обработки оператора >
        {
            int count = Postfix.Count;
            if (count < 2) return;
            var temp = Postfix[count - 1];
            Postfix[count - 1] = Postfix[count - 2];
            Postfix[count - 2] = temp;
        }

        public static bool DoUntilStatement()
        {
            Log("Вход в DoUntilStatement, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
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

            int loopStart = Postfix.Count;
            if (!LogicalExpression())
                return false;
            WriteCmd(ECmd.NOT);
            int exitJumpIndex = WriteCmdPtr(-1);
            WriteCmd(ECmd.JZ);

            if (!Operators())
                return false;

            WriteCmdPtr(loopStart);
            int jmpIndex = WriteCmd(ECmd.JMP);
            SetCmdPtr(exitJumpIndex, jmpIndex + 1);

            if (CurrentToken == null || CurrentToken.Type != TokenType.Loop)
            {
                Error("Ожидалось 'loop'", CurrentToken != null ? CurrentToken.StartPos : -1);
                return false;
            }
            currentIndex++;

            indentLevel--;
            Log("Выход из DoUntilStatement, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            if (currentIndex < tokens.Count)
            {
                Error("Лишние токены после 'loop'", CurrentToken != null ? CurrentToken.StartPos : -1);
                return false;
            }
            return true;
        }

        public static bool LogicalExpression()
        {
            Log("Вход в LogicalExpression, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;

            bool hasNot = false;
            if (CurrentToken != null && CurrentToken.Type == TokenType.Not)
            {
                hasNot = true;
                Log("Найден 'not'");
                currentIndex++;
            }

            if (!ComparisonExpression())
                return false;

            if (hasNot)
            {
                WriteCmd(ECmd.NOT);
            }

            while (CurrentToken != null &&
                   (CurrentToken.Type == TokenType.And || CurrentToken.Type == TokenType.Or))
            {
                Token op = CurrentToken;
                Log("Найден логический оператор: " + op.ToString());
                currentIndex++;

                if (!ComparisonExpression())
                    return false;

                if (op.Type == TokenType.And)
                    WriteCmd(ECmd.AND);
                else if (op.Type == TokenType.Or)
                    WriteCmd(ECmd.OR);
            }
            indentLevel--;
            Log("Выход из LogicalExpression, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        public static bool ComparisonExpression()
        {
            Log("Вход в ComparisonExpression, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (!Operand())
                return false;

            if (CurrentToken != null && CurrentToken.Type == TokenType.Rel)
            {
                string op = CurrentToken.Lexeme;
                Log("Найден оператор сравнения: " + op);
                currentIndex++;
                if (!Operand())
                    return false;

                ECmd cmd;
                if (op == "<")
                    cmd = ECmd.CMPL;
                else if (op == ">")
                {
                    SwapLastTwoEntries();
                    cmd = ECmd.CMPL;
                }
                else if (op == "==")
                    cmd = ECmd.CMPE;
                else if (op == "<>")
                    cmd = ECmd.CMPNE;
                else
                    cmd = ECmd.CMPL;

                WriteCmd(cmd);
            }
            indentLevel--;
            Log("Выход из ComparisonExpression, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        public static bool OperatorStmt()
        {
            Log("Вход в OperatorStmt, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (CurrentToken == null)
            {
                Error("Ожидался оператор", -1);
                return false;
            }
            if (CurrentToken.Type == TokenType.Identifier)
            {
                string varName = CurrentToken.Lexeme;
                int varIndex = Identifiers.IndexOf(varName);
                Log("Обнаружено присваивание, идентификатор: " + CurrentToken.ToString());
                WriteVar(varIndex);
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
                WriteCmd(ECmd.SET);
                indentLevel--;
                Log("Выход из OperatorStmt (присваивание), токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
                return true;
            }
            else if (CurrentToken.Type == TokenType.Output)
            {
                Log("Обнаружен оператор вывода: " + CurrentToken.ToString());
                currentIndex++;
                if (!Operand())
                    return false;
                WriteCmd(ECmd.OUTPUT);
                indentLevel--;
                Log("Выход из OperatorStmt (вывод), токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
                return true;
            }
            else
            {
                Error("Ожидалось присваивание или оператор вывода", CurrentToken.StartPos);
                return false;
            }
        }

        public static bool ArithExpr()
        {
            Log("Вход в ArithExpr, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (!Operand())
                return false;

            while (CurrentToken != null &&
                   (CurrentToken.Type == TokenType.Plus ||
                    CurrentToken.Type == TokenType.Minus ||
                    CurrentToken.Type == TokenType.Multiply ||
                    CurrentToken.Type == TokenType.Divide))
            {
                Token op = CurrentToken;
                Log("Найден арифметический оператор: " + op.ToString());
                currentIndex++;
                if (!Operand())
                    return false;
                ECmd cmd;
                if (op.Type == TokenType.Plus)
                    cmd = ECmd.ADD;
                else if (op.Type == TokenType.Minus)
                    cmd = ECmd.SUB;
                else if (op.Type == TokenType.Multiply)
                    cmd = ECmd.MUL;
                else
                    cmd = ECmd.DIV;
                WriteCmd(cmd);
            }
            indentLevel--;
            Log("Выход из ArithExpr, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        public static bool Operand()
        {
            Log("Вход в Operand, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            indentLevel++;
            if (CurrentToken == null)
            {
                Error("Ожидался операнд (идентификатор или константа)", -1);
                return false;
            }
            if (CurrentToken.Type == TokenType.Identifier)
            {
                string varName = CurrentToken.Lexeme;
                int varIndex = Identifiers.IndexOf(varName);
                Log("Найден идентификатор: " + CurrentToken.ToString());
                WriteVar(varIndex);
                currentIndex++;
            }
            else if (CurrentToken.Type == TokenType.Constant)
            {
                string constVal = CurrentToken.Lexeme;
                int constIndex = Constants.IndexOf(constVal);
                Log("Найдена константа: " + CurrentToken.ToString());
                WriteConst(constIndex);
                currentIndex++;
            }
            else
            {
                Error("Ожидался идентификатор или константа", CurrentToken.StartPos);
                return false;
            }
            indentLevel--;
            Log("Выход из Operand, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        public static bool Operators()
        {
            Log("Вход в Operators, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
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
            Log("Выход из Operators, токен: " + (CurrentToken != null ? CurrentToken.ToString() : "null"));
            return true;
        }

        public static bool Parse(List<Token> tokenList, List<string> idTable, List<string> constTable)
        {
            tokens = tokenList;
            Identifiers = idTable;
            Constants = constTable;
            currentIndex = 0;
            parseLog.Clear();
            indentLevel = 0;
            Postfix.Clear();

            Log("=== Начало синтаксического анализа ===");
            bool result = DoUntilStatement();
            Log("=== Конец синтаксического анализа ===");
            return result;
        }
    }
}
