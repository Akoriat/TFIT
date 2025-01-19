using System;
using System.Collections.Generic;
using Lab4.Models;

namespace Lab4.Services
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        private IdentifierTable _idTable;
        private ConstantTable _constTable;

        private List<PostfixEntry> _postfix = new List<PostfixEntry>();

        public Parser(List<Token> tokens, IdentifierTable idTable, ConstantTable cTable)
        {
            _tokens = tokens;
            _pos = 0;
            _idTable = idTable;
            _constTable = cTable;
        }

        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : null;

        private bool Error(string msg)
        {
            Console.WriteLine($"Ошибка: {msg} (pos={Current?.Position}, lex='{Current?.Lexeme}')");
            return false;
        }

        private void NextToken()
        {
            if (_pos < _tokens.Count) _pos++;
        }

        //------------------------------------------------------------------------
        // Методы записи в ПОЛИЗ
        //------------------------------------------------------------------------

        private int WriteCmd(ECmd cmd)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etCmd, (int)cmd));
            return idx;
        }

        /// <summary>
        /// Записываем переменную (индекс) для левой части присваивания
        /// (в интерпретаторе это case etVar => push index).
        /// </summary>
        private int WriteVar(int varIndex)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etVar, varIndex));
            return idx;
        }

        /// <summary>
        /// Записываем константу
        /// </summary>
        private int WriteConst(int cIndex)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etConst, cIndex));
            return idx;
        }

        /// <summary>
        /// Записываем адрес (число) для переходов/LOAD
        /// </summary>
        private int WriteCmdPtr(int val)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etCmdPtr, val));
            return idx;
        }

        private void SetCmdPtr(int atIndex, int ptrValue)
        {
            PostfixEntry entry = _postfix[atIndex];
            entry.index = ptrValue;
            _postfix[atIndex] = entry;
        }

        //------------------------------------------------------------------------
        // НОВЫЙ метод: WriteLoadVar(varIndex)
        // -> генерируем (LOAD), (CmdPtr(varIndex))
        //------------------------------------------------------------------------
        private int WriteLoadVar(int varIndex)
        {
            // 1) etCmd(LOAD)
            int loadPos = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etCmd, (int)ECmd.LOAD));

            // 2) etCmdPtr(varIndex)
            int ptrPos = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etCmdPtr, varIndex));

            return loadPos;
        }

        //------------------------------------------------------------------------
        // Главная точка входа
        //------------------------------------------------------------------------
        public bool ParseProgram()
        {
            // Для простоты: парсим 1 оператор 
            if (!ParseStatement()) return false;

            // Если хотим много операторов - можно цикл while ...
            if (_pos < _tokens.Count)
            {
                Console.WriteLine("Предупреждение: есть лишние лексемы после первого оператора");
            }
            return true;
        }

        /// <summary>
        /// Определяем, какой оператор: while/do/присваивание
        /// </summary>
        private bool ParseStatement()
        {
            if (Current == null)
                return Error("Ожидался оператор, но вход закончился");

            if (Current.Type == TokenType.While)
            {
                return ParseWhileStatement();
            }
            else if (Current.Type == TokenType.Do)
            {
                return ParseDoUntilStatement();
            }
            else if (Current.Type == TokenType.Var)
            {
                // присваивание
                return ParseAssignStatement();
            }
            else
            {
                return Error($"Неизвестный оператор: {Current.Lexeme}");
            }
        }

        //------------------------------------------------------------------------
        // while <Condition> do <Statement> end
        //------------------------------------------------------------------------
        public bool ParseWhileStatement()
        {
            if (Current == null || Current.Type != TokenType.While)
                return Error("Ожидался 'while'");
            NextToken();

            int startPos = _postfix.Count;

            // <Condition>
            if (!ParseCondition()) return false;

            // вставляем JZ
            int jzPtrIndex = WriteCmdPtr(-1);
            WriteCmd(ECmd.JZ);

            // do
            if (Current == null || Current.Type != TokenType.Do)
                return Error("Ожидался 'do'");
            NextToken();

            // тело цикла
            if (!ParseStatement()) return false;

            // end
            if (Current == null || Current.Type != TokenType.End)
                return Error("Ожидался 'end'");
            NextToken();

            // Переход назад
            WriteCmdPtr(startPos);
            int jmpPos = WriteCmd(ECmd.JMP);

            // fix JZ
            SetCmdPtr(jzPtrIndex, jmpPos + 1);

            return true;
        }

        //------------------------------------------------------------------------
        // do until <Condition> <Operators> loop
        //------------------------------------------------------------------------
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

            // тело цикла
            if (!ParseStatement()) return false;

            // loop
            if (Current == null || Current.Type != TokenType.Loop)
                return Error("Ожидался 'loop'");
            NextToken();

            // Переход назад
            WriteCmdPtr(startPos);
            int jmpPos = WriteCmd(ECmd.JMP);

            // Fix JZ
            SetCmdPtr(jzPtrIndex, jmpPos + 1);

            return true;
        }

        //------------------------------------------------------------------------
        // Присваивание: x = <ArithExpr>
        //------------------------------------------------------------------------
        private bool ParseAssignStatement()
        {
            if (Current == null || Current.Type != TokenType.Var)
                return Error("Ожидался идентификатор");
            int vIndex = Current.Index;
            NextToken();

            if (Current == null || Current.Type != TokenType.As)
                return Error("Ожидался '='");
            NextToken();

            // слева varIndex
            WriteVar(vIndex);

            // правая часть
            if (!ParseArithExpr()) return false;

            // SET
            WriteCmd(ECmd.SET);
            return true;
        }

        //------------------------------------------------------------------------
        // <ArithExpr> -> <Operand> { Ao <Operand> }
        //------------------------------------------------------------------------
        private bool ParseArithExpr()
        {
            if (!ParseOperand()) return false;

            while (Current != null && Current.Type == TokenType.Ao)
            {
                ECmd cmd = ECmd.ADD;
                if (Current.Lexeme == "-")
                    cmd = ECmd.SUB;
                NextToken();

                if (!ParseOperand()) return false;
                WriteCmd(cmd);
            }
            return true;
        }

        //------------------------------------------------------------------------
        // <Operand> -> var | const
        //  теперь var => LOAD varIndex
        //------------------------------------------------------------------------
        private bool ParseOperand()
        {
            if (Current == null)
                return Error("Ожидался операнд");

            if (Current.Type == TokenType.Var)
            {
                // Чтение значения переменной
                WriteLoadVar(Current.Index);
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

        //------------------------------------------------------------------------
        // Логические выражения
        //------------------------------------------------------------------------
        private bool ParseCondition()
        {
            // <LogExpr> { or <LogExpr> }
            if (!ParseLogExpr()) return false;
            while (Current != null && Current.Type == TokenType.Or)
            {
                NextToken();
                if (!ParseLogExpr()) return false;
                WriteCmd(ECmd.OR);
            }
            return true;
        }

        private bool ParseLogExpr()
        {
            // <RelExpr> { and <RelExpr> }
            if (!ParseRelExpr()) return false;
            while (Current != null && Current.Type == TokenType.And)
            {
                NextToken();
                if (!ParseRelExpr()) return false;
                WriteCmd(ECmd.AND);
            }
            return true;
        }

        private bool ParseRelExpr()
        {
            // <Operand> [rel <Operand>]
            if (!ParseOperand()) return false;

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

                if (!ParseOperand()) return false;
                WriteCmd(cmd);
            }
            return true;
        }

        //------------------------------------------------------------------------
        public IReadOnlyList<PostfixEntry> GetPostfix() => _postfix;
    }
}
