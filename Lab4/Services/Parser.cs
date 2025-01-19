using System;
using System.Collections.Generic;
using Lab4.Models;

namespace Lab4.Services
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        // Таблицы (для var/const)
        private IdentifierTable _idTable;
        private ConstantTable _constTable;

        // ПОЛИЗ
        private List<PostfixEntry> _postfix = new List<PostfixEntry>();

        public Parser(List<Token> tokens, IdentifierTable idTable, ConstantTable cTable)
        {
            _tokens = tokens;
            _pos = 0;
            _idTable = idTable;
            _constTable = cTable;
        }

        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : null;

        private bool Error(string message)
        {
            // Можно выводить в консоль или бросать Exception
            Console.WriteLine($"Ошибка: {message} (pos={Current?.Position}, lex='{Current?.Lexeme}')");
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

        private int WriteVar(int varIndex)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etVar, varIndex));
            return idx;
        }

        private int WriteConst(int constIndex)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etConst, constIndex));
            return idx;
        }

        private int WriteCmdPtr(int ptrVal)
        {
            int idx = _postfix.Count;
            _postfix.Add(new PostfixEntry(EEntryType.etCmdPtr, ptrVal));
            return idx;
        }

        private void SetCmdPtr(int atIndex, int ptrValue)
        {
            PostfixEntry entry = _postfix[atIndex];
            entry.index = ptrValue;
            _postfix[atIndex] = entry;
        }

        //------------------------------------------------------------------------
        // Точка входа
        //------------------------------------------------------------------------
        // В вашем случае вы можете вызвать ParseStatement() из MainWindow.xaml.cs
        // или сделать отдельный ParseProgram(), который парсит последовательность
        // операторов, или конкретно while/doUntil. Ниже - пример для одного оператора:

        public bool ParseWhileStatement() // <-- Существовал метод, не меняем
        {
            // <WhileStatement> → while <Condition> do <Statement> end

            if (Current == null || Current.Type != TokenType.While)
                return Error("Ожидался 'while'");
            NextToken();

            int startPos = _postfix.Count; // начало цикла

            // <Condition>
            if (!ParseCondition()) return false;

            // запишем фиктивный адрес
            int jzPtrIndex = WriteCmdPtr(-1);
            // команда JZ
            WriteCmd(ECmd.JZ);

            // do
            if (Current == null || Current.Type != TokenType.Do)
                return Error("Ожидался 'do'");
            NextToken();

            // <Statement> (или <Operators>)
            // Если хотим несколько операторов - можно вызвать ParseOperators()
            if (!ParseStatement()) return false;

            // end
            if (Current == null || Current.Type != TokenType.End)
                return Error("Ожидался 'end'");
            NextToken();

            // Записываем адрес начала
            WriteCmdPtr(startPos);
            int jmpPos = WriteCmd(ECmd.JMP);

            // Подставляем реальный адрес конца (jmpPos+1)
            SetCmdPtr(jzPtrIndex, jmpPos + 1);

            return true;
        }

        //------------------------------------------------------------------------
        // ДОБАВЛЕННЫЙ МЕТОД: do until <Condition> <Operators> loop
        //------------------------------------------------------------------------
        private bool ParseDoUntilStatement()
        {
            // Ожидаем 'do'
            if (Current == null || Current.Type != TokenType.Do)
                return Error("Ожидалось 'do'");
            NextToken(); // съели 'do'

            // Ожидаем 'until'
            if (Current == null || Current.Type != TokenType.Until)
                return Error("Ожидалось 'until'");
            NextToken(); // съели 'until'

            // Запомним начало тела (адрес в ПОЛИЗ)
            int startPos = _postfix.Count;

            // Парсим <Condition> (генерация ПОЛИЗа для лог. выражения)
            if (!ParseCondition()) return false;

            // Дополнительно вставляем команду NOT
            // (т.к. do until cond == пока cond==false => while !cond)
            WriteCmd(ECmd.NOT);

            // Запишем фиктивный адрес, вернём индекс
            int jzPtrIndex = WriteCmdPtr(-1);
            WriteCmd(ECmd.JZ);

            // Теперь парсим <операторы> (или 1 оператор). 
            // В зависимости от вашей грамматики, это может быть ParseOperators()
            if (!ParseOperators()) return false;

            // Ожидаем 'loop'
            if (Current == null || Current.Type != TokenType.Loop)
                return Error("Ожидалось 'loop'");
            NextToken(); // съели 'loop'

            // Вставляем переход назад на startPos
            WriteCmdPtr(startPos);
            int jmpPos = WriteCmd(ECmd.JMP);

            // Подставляем реальный адрес конца (jmpPos + 1)
            SetCmdPtr(jzPtrIndex, jmpPos + 1);

            return true;
        }

        //------------------------------------------------------------------------
        // Дополнительная логика для логического условия
        //------------------------------------------------------------------------

        private bool ParseCondition()
        {
            // <Condition> -> <LogExpr> { or <LogExpr> }
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
            // <LogExpr> -> <RelExpr> { and <RelExpr> }
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
            // <RelExpr> -> <Operand> [ rel <Operand> ]
            if (!ParseOperand()) return false;

            if (Current != null && Current.Type == TokenType.Rel)
            {
                // Определяем, какая команда: <, <=, ==, <>, ...
                ECmd cmd = ECmd.CMPE;
                switch (Current.Lexeme)
                {
                    case "<": cmd = ECmd.CMPL; break;
                    case "<=": cmd = ECmd.CMPLE; break;
                    case "==": cmd = ECmd.CMPE; break;
                    case "<>": cmd = ECmd.CMPNE; break;
                        // при желании: case ">": ..., case ">=": ...
                }
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

        //------------------------------------------------------------------------
        // Операторы
        //------------------------------------------------------------------------

        /// <summary>
        /// Парсим последовательность операторов (минимум один).
        /// Если нужно, можно расширить, чтобы обрабатывать ";", несколько операторов.
        /// </summary>
        private bool ParseOperators()
        {
            // Пример: парсим хотя бы один оператор:
            if (!ParseStatement()) return false;

            // Если у вас операторы разделяются ;, можно сделать так:
            // while (Current != null && Current.Type == TokenType.Semicolon)
            // {
            //    NextToken();
            //    if (!ParseStatement()) return false;
            // }
            return true;
        }

        /// <summary>
        /// Универсальный метод, выбирающий, какой оператор сейчас парсить:
        /// - while <Condition> do ...
        /// - do until <Condition> ...
        /// - var as <ArithExpr>
        /// И так далее. 
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
                // Может быть do until ...
                return ParseDoUntilStatement();
            }
            else if (Current.Type == TokenType.Var)
            {
                // Считаем это присваиванием var = expr
                return ParseAssignStatement();
            }
            else
            {
                return Error($"Неизвестный оператор. Лексема: {Current.Lexeme}");
            }
        }

        private bool ParseAssignStatement()
        {
            // <Statement> -> var as <ArithExpr>
            if (Current == null || Current.Type != TokenType.Var)
                return Error("Ожидался идентификатор (var)");
            int vIndex = Current.Index;
            NextToken();

            if (Current == null || Current.Type != TokenType.As)
                return Error("Ожидался оператор 'as'");
            NextToken();

            // Ставим var
            WriteVar(vIndex);

            // <ArithExpr>
            if (!ParseArithExpr()) return false;

            // SET
            WriteCmd(ECmd.SET);
            return true;
        }

        private bool ParseArithExpr()
        {
            // <ArithExpr> -> <Operand> { ao <Operand> }
            if (!ParseOperand()) return false;

            while (Current != null && Current.Type == TokenType.Ao)
            {
                // +, -
                ECmd cmd = ECmd.ADD;
                if (Current.Lexeme == "-")
                    cmd = ECmd.SUB;
                // (Если надо * /, добавляйте)

                NextToken();

                if (!ParseOperand()) return false;
                WriteCmd(cmd);
            }
            return true;
        }

        //------------------------------------------------------------------------
        // Доступ к результату - ПОЛИЗ
        //------------------------------------------------------------------------
        public IReadOnlyList<PostfixEntry> GetPostfix() => _postfix;
    }
}
