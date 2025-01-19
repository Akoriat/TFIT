using System;
using System.Collections.Generic;
using Lab3.Models;

namespace Lab3.Services
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        // Таблицы (если нужны):
        private IdentifierTable _idTable;
        private ConstantTable _constTable;

        // ПОЛИЗ
        private List<PostfixEntry> _postfix = new List<PostfixEntry>();

        public Parser(List<Token> tokens, IdentifierTable idTable, ConstantTable constTable)
        {
            _tokens = tokens;
            _pos = 0;
            _idTable = idTable;
            _constTable = constTable;
        }

        // Доступ к текущей лексеме
        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : null;

        private bool Error(string msg)
        {
            if (Current != null)
                Console.WriteLine($"Ошибка: {msg} (pos {Current.Position}, lexeme '{Current.Lexeme}')");
            else
                Console.WriteLine($"Ошибка: {msg} (конец лексем)");
            return false;
        }

        private void NextToken() { if (_pos < _tokens.Count) _pos++; }

        // Методы записи в ПОЛИЗ
        private int WriteCmd(ECmd cmd)
        {
            int pos = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etCmd, (int)cmd));
            return pos;
        }

        private int WriteVar(int varIndex)
        {
            int pos = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etVar, varIndex));
            return pos;
        }

        private int WriteConst(int constIndex)
        {
            int pos = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etConst, constIndex));
            return pos;
        }

        private int WriteCmdPtr(int ptrValue)
        {
            int pos = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etCmdPtr, ptrValue));
            return pos;
        }

        private void SetCmdPtr(int atIndex, int ptrValue)
        {
            PostfixEntry entry = _postfix[atIndex];
            entry.index = ptrValue;
            _postfix[atIndex] = entry;
        }

        public bool ParseWhileStatement()
        {
            // Пример: <WhileStatement> → while <Condition> do <Statement> end
            // Генерация ПОЛИЗа:
            //   <условие>  [инд_адр_JZ] JZ   <Statement> [инд_адр_старта] JMP
            //   (потом подставляем реальные адреса)

            int startIndex = _postfix.Count; // адрес начала цикла

            // 1) while
            if (Current == null || Current.Type != TokenType.While)
                return Error("Ожидался 'while'");
            NextToken();

            // 2) <Condition>
            if (!ParseCondition()) return false;

            // Запишем фиктивный адрес для JZ
            int jzPtrIndex = WriteCmdPtr(-1);
            // Запишем команду JZ
            WriteCmd(ECmd.JZ);

            // 3) do
            if (Current == null || Current.Type != TokenType.Do)
                return Error("Ожидался 'do'");
            NextToken();

            // 4) <Statement>
            if (!ParseStatement()) return false;

            // 5) end
            if (Current == null || Current.Type != TokenType.End)
                return Error("Ожидался 'end'");
            NextToken();

            // Запишем адрес начала цикла (startIndex)
            WriteCmdPtr(startIndex);
            // Запишем команду JMP
            int jmpPos = WriteCmd(ECmd.JMP);

            // Теперь подставим корректный адрес для JZ
            // Переход должен вестись на jmpPos+1 (т. е. на ячейку, куда пойдёт исполнение после JMP)
            SetCmdPtr(jzPtrIndex, jmpPos + 1);

            // Если остались лексемы
            if (_pos < _tokens.Count)
            {
                return Error("Лишние символы после 'end'");
            }
            return true;
        }

        private bool ParseCondition()
        {
            // <Condition> → <LogExpr> { or <LogExpr> }
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
            // <LogExpr> → <RelExpr> { and <RelExpr> }
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
            // <RelExpr> → <Operand> [rel <Operand>]
            if (!ParseOperand()) return false;

            if (Current != null && Current.Type == TokenType.Rel)
            {
                // Определяем, какая операция сравнения
                ECmd cmd = ECmd.CMPE; // default
                switch (Current.Index)
                {
                    case 5: cmd = ECmd.CMPL; break;  // <  (пример по таблице)
                    case 6: cmd = ECmd.CMPLE; break;  // <=
                    case 7: cmd = ECmd.CMPNE; break;  // <>
                    case 8: cmd = ECmd.CMPE; break;  // ==
                                                     // ... если есть ещё ...
                }
                NextToken();

                if (!ParseOperand()) return false;
                WriteCmd(cmd);
            }
            return true;
        }

        private bool ParseOperand()
        {
            if (Current == null)
                return Error("Ожидался операнд");

            if (Current.Type == TokenType.Var)
            {
                WriteVar(Current.Index); // index в таблице идентификаторов
                NextToken();
                return true;
            }
            else if (Current.Type == TokenType.Const)
            {
                WriteConst(Current.Index); // index в таблице констант
                NextToken();
                return true;
            }
            else
            {
                return Error("Ожидался var или const");
            }
        }

        private bool ParseStatement()
        {
            // <Statement> → var as <ArithExpr>
            if (Current == null || Current.Type != TokenType.Var)
                return Error("Ожидался идентификатор (var)");
            // Ставим var в ПОЛИЗ (кому присваиваем)
            WriteVar(Current.Index);
            NextToken();

            // as
            if (Current == null || Current.Type != TokenType.As)
                return Error("Ожидалась операция 'as'");
            NextToken();

            // <ArithExpr>
            if (!ParseArithExpr()) return false;

            // в конце ставим команду SET
            WriteCmd(ECmd.SET);

            return true;
        }

        private bool ParseArithExpr()
        {
            // <ArithExpr> → <Operand> { ao <Operand> }
            if (!ParseOperand()) return false;

            while (Current != null && Current.Type == TokenType.Ao)
            {
                // Нужно определить, ADD или SUB (или MUL, DIV...)
                ECmd cmd = ECmd.ADD; // default
                // По Index лексемы определяем, что за символ (например, + или -)
                // Допустим + имеет index=10, - =11 (как в примере)
                if (Current.Index == 11)
                    cmd = ECmd.SUB;
                NextToken();

                if (!ParseOperand()) return false;
                WriteCmd(cmd);
            }
            return true;
        }

        // Позволяем извне получить результат
        public IReadOnlyList<PostfixEntry> GetPostfix() => _postfix;
    }
}
