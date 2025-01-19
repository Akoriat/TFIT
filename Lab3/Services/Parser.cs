using System;
using System.Collections.Generic;
using Lab3.Models;

namespace Lab3.Services
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        // Список PostfixEntry (ПОЛИЗ)
        private List<PostfixEntry> _postfix = new List<PostfixEntry>();

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
        }

        private Token Current => (_pos < _tokens.Count) ? _tokens[_pos] : null;

        private bool Error(string msg)
        {
            Console.WriteLine($"Ошибка: {msg}, near='{Current?.Lexeme}', pos={Current?.Position}");
            return false;
        }

        private void NextToken()
        {
            if (_pos < _tokens.Count) _pos++;
        }

        // Методы записи
        private int WriteCmd(ECmd cmd)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etCmd, (int)cmd));
            return idx;
        }

        private int WriteVar(int varIndex)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etVar, varIndex));
            return idx;
        }

        private int WriteConst(int cIndex)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etConst, cIndex));
            return idx;
        }

        private int WriteCmdPtr(int val)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etCmdPtr, val));
            return idx;
        }

        private void SetCmdPtr(int atIndex, int newVal)
        {
            PostfixEntry e = _postfix[atIndex];
            e.index = newVal;
            _postfix[atIndex] = e;
        }

        //=== Парсим 1 оператор/конструкцию ===
        public bool ParseProgram()
        {
            if (!ParseStatement()) return false;

            if (_pos < _tokens.Count)
            {
                Console.WriteLine("Предупреждение: есть лишние лексемы после оператора");
            }
            return true;
        }

        private bool ParseStatement()
        {
            // опред: while, do, var, ...
            if (Current == null) return Error("Ожидался оператор.");

            if (Current.Type == TokenType.While)
                return ParseWhileStatement();
            else if (Current.Type == TokenType.Do)
                return ParseDoUntilStatement();
            else if (Current.Type == TokenType.Var)
                return ParseAssignStatement();
            else
                return Error($"Неизвестный оператор '{Current.Lexeme}'");
        }

        private bool ParseOperators()
        {
            if (!ParseStatement()) return false;
            return true; // упрощённо
        }

        //=== while <Condition> do <Operators> end
        private bool ParseWhileStatement()
        {
            if (Current == null || Current.Type != TokenType.While)
                return Error("Ожидался 'while'");
            NextToken();

            int startPos = _postfix.Count;

            // <Condition>
            if (!ParseCondition()) return false;

            // Записать cmdPtr(-1) + JZ
            int jzPtrIndex = WriteCmdPtr(-1);
            WriteCmd(ECmd.JZ);

            // do
            if (Current == null || Current.Type != TokenType.Do)
                return Error("Ожидался 'do'");
            NextToken();

            // тело
            if (!ParseOperators()) return false;

            // end
            if (Current == null || Current.Type != TokenType.End)
                return Error("Ожидался 'end'");
            NextToken();

            // JMP start
            WriteCmdPtr(startPos);
            int jmpPos = WriteCmd(ECmd.JMP);

            // fix jz => jmpPos+1
            SetCmdPtr(jzPtrIndex, jmpPos + 1);

            return true;
        }

        //=== do until <Condition> <operators> loop
        private bool ParseDoUntilStatement()
        {
            // do
            if (Current == null || Current.Type != TokenType.Do)
                return Error("Ожидался 'do'");
            NextToken();

            // until
            if (Current == null || Current.Type != TokenType.Until)
                return Error("Ожидался 'until'");
            NextToken();

            int startPos = _postfix.Count;

            // <Condition>
            if (!ParseCondition()) return false;

            // NOT
            WriteCmd(ECmd.NOT);

            // JZ
            int jzPtrIndex = WriteCmdPtr(-1);
            WriteCmd(ECmd.JZ);

            // operators
            if (!ParseOperators()) return false;

            // loop
            if (Current == null || Current.Type != TokenType.Loop)
                return Error("Ожидался 'loop'");
            NextToken();

            // JMP start
            WriteCmdPtr(startPos);
            int jmpPos = WriteCmd(ECmd.JMP);

            // fix jz
            SetCmdPtr(jzPtrIndex, jmpPos + 1);

            return true;
        }

        //=== Присваивание: var as <ArithExpr>
        private bool ParseAssignStatement()
        {
            if (Current == null || Current.Type != TokenType.Var)
                return Error("Ожидался идентификатор");
            int vIndex = Current.Index;
            NextToken();

            if (Current == null || Current.Type != TokenType.As)
                return Error("Ожидался '='");
            NextToken();

            // слева
            WriteVar(vIndex);

            // парсим выражение
            if (!ParseArithExpr()) return false;

            // SET
            WriteCmd(ECmd.SET);

            return true;
        }

        //=== <Condition> -> <LogExpr> { or <LogExpr> }
        private bool ParseCondition()
        {
            if (!ParseLogExpr()) return false;
            while (Current != null && Current.Type == TokenType.Or)
            {
                NextToken();
                if (!ParseLogExpr()) return false;
                WriteCmd(ECmd.OR);
            }
            return true;
        }

        //=== <LogExpr> -> <RelExpr> { and <RelExpr> }
        private bool ParseLogExpr()
        {
            if (!ParseRelExpr()) return false;
            while (Current != null && Current.Type == TokenType.And)
            {
                NextToken();
                if (!ParseRelExpr()) return false;
                WriteCmd(ECmd.AND);
            }
            return true;
        }

        //=== <RelExpr> -> <ArithExpr> [ rel <ArithExpr> ]
        private bool ParseRelExpr()
        {
            if (!ParseArithExpr()) return false;

            if (Current != null && Current.Type == TokenType.Rel)
            {
                ECmd cmd = ECmd.CMPE;
                switch (Current.Lexeme)
                {
                    case "<": cmd = ECmd.CMPL; break;
                    case "<=": cmd = ECmd.CMPLE; break;
                    case "==": cmd = ECmd.CMPE; break;
                    case "<>": cmd = ECmd.CMPNE; break;
                }
                NextToken();

                if (!ParseArithExpr()) return false;
                WriteCmd(cmd);
            }
            return true;
        }

        //=== <ArithExpr> -> <Operand> { Ao <Operand> }
        private bool ParseArithExpr()
        {
            if (!ParseOperand()) return false;

            while (Current != null && Current.Type == TokenType.Ao)
            {
                ECmd cmd = ECmd.ADD;
                if (Current.Lexeme == "-") cmd = ECmd.SUB;
                NextToken();

                if (!ParseOperand()) return false;
                WriteCmd(cmd);
            }
            return true;
        }

        private bool ParseOperand()
        {
            // <Operand> -> var | const
            if (Current == null)
                return Error("Ожидался операнд");

            if (Current.Type == TokenType.Var)
            {
                WriteVar(Current.Index);
                NextToken();
                return true;
            }
            else if (Current.Type == TokenType.Const)
            {
                WriteConst(Current.Index);
                NextToken();
                return true;
            }
            else
            {
                return Error("Ожидался var или const");
            }
        }

        public IReadOnlyList<PostfixEntry> GetPostfix() => _postfix;
    }
}
